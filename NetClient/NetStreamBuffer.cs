﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System;
using System.Threading.Tasks;

namespace Framework.NetWork
{
    /// <summary>
    /// wrapper of NetworkStream, be responsible for sending/receiving of protocol
    /// </summary>
    internal class NetStreamBuffer
    {
        private const int       m_MinCapacity = 1024;
        private NetworkStream   m_Stream;
        private TcpClient       m_Client;

        private byte[]          m_Buffer;
        private int             m_Head;
        private int             m_Tail;
        private int             m_IndexMask;

        public byte[]           Buffer  { get { return m_Buffer; } }

        public int              Head    { get { return m_Head; } }

        public int              Tail    { get { return m_Tail; } }

        public NetStreamBuffer(TcpClient client, int capacity = 8 * 1024)
        {
            if (client == null) throw new ArgumentNullException();

            m_Client = client;
            m_Stream = client.GetStream();
            EnsureCapacity(capacity);
        }

        private void Clear()
        {
            m_Head = 0;
            m_Tail = 0;
        }

        internal bool IsEmpty()
        {
            return m_Head == m_Tail;
        }

        internal bool IsFull()
        {
            return ((m_Head + 1) & m_IndexMask) == m_Tail;
        }

        private int GetFreeCapacity()
        {
            return m_Buffer.Length - 1 - GetUsedCapacity();
        }

        private int GetUsedCapacity()
        {
            return m_Head >= m_Tail ? m_Head - m_Tail : m_Buffer.Length - 1 - (m_Tail - m_Head);
        }

        public bool Write(byte[] data, int offset, int length)
        {
            if (data == null)
                throw new ArgumentNullException("data == null");

            // 传入参数的合法性检查:可写入空间大小的检查
            if (offset + length > data.Length || length > GetFreeCapacity())
                throw new ArgumentOutOfRangeException();

            if(m_Head + length <= m_Buffer.Length - 1)              // buffer最后一个字节留空
            {
                System.Buffer.BlockCopy(data, offset, m_Buffer, m_Head, length);
            }
            else
            {
                int countToEnd = m_Buffer.Length - 1 - m_Head;      // 到buffer末尾的剩余空间，最后一个字节留空
                System.Buffer.BlockCopy(data, offset, m_Buffer, m_Head, countToEnd);
                System.Buffer.BlockCopy(data, countToEnd, m_Buffer, 0, length - countToEnd);
            }
            m_Head = (m_Head + length) & m_IndexMask;

            return true;
        }

        public bool Write(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException("data == null");
            return Write(data, 0, data.Length);
        }

        private int NextPowerOfTwo(int n)
        {
            n--;
            n |= n >> 1;
            n |= n >> 2;
            n |= n >> 4;
            n |= n >> 8;
            n |= n >> 16;
            n++;
            return n;
        }

        private void EnsureCapacity(int min)
        {
            if(m_Buffer.Length < min)
            {
                int newCapacity = m_Buffer.Length == 0 ? m_MinCapacity : m_Buffer.Length * 2;
                if((uint)newCapacity > Int32.MaxValue)
                    newCapacity = Int32.MaxValue;
                if(newCapacity < min)
                    newCapacity = min;
                newCapacity = NextPowerOfTwo(newCapacity);

                // expand buffer
                byte[] newBuf = new byte[newCapacity];
                int length = GetUsedCapacity();
                if(m_Head > m_Tail)
                {
                    System.Buffer.BlockCopy(m_Buffer, 0, newBuf, 0, m_Head - m_Tail);
                }
                else if(m_Head < m_Tail)
                {
                    System.Buffer.BlockCopy(m_Buffer, m_Tail, newBuf, 0, m_Buffer.Length - 1 - m_Tail);
                    System.Buffer.BlockCopy(m_Buffer, 0, newBuf, m_Buffer.Length - 1 - m_Tail, m_Head);
                }
                m_Buffer = newBuf;
                m_Tail = 0;
                m_Head = length;
                m_IndexMask = m_Buffer.Length - 1;
            }
        }

        /// <summary>
        /// 异步发送Buff所有数据，由上层决定什么时候发送（最佳实践：一帧调用一次）
        /// </summary>
        /// <returns></returns>
        public async Task FlushWrite()
        {
            try
            {
                if (IsEmpty() || !m_Stream.CanWrite)
                    return;

                int count = GetUsedCapacity();
                if (m_Head > m_Tail)
                {
                    await m_Stream.WriteAsync(m_Buffer, m_Tail, count);
                }
                else
                {
                    await m_Stream.WriteAsync(m_Buffer, m_Tail, m_Buffer.Length -1 - m_Tail);
                    if(m_Head > 0)
                        await m_Stream.WriteAsync(m_Buffer, 0, m_Head);
                }

                m_Tail = (m_Tail + count) & m_IndexMask;        // 数据发送完成，更新Tail
            }
            catch (ObjectDisposedException e)
            {
                // The NetworkStream is closed
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("The buffer parameter is NULL");
            }
            catch(ArgumentOutOfRangeException e)
            {
                Console.WriteLine(e.Message);
            }
            catch(InvalidOperationException e)
            {
                //Console.WriteLine("The NetworkStream does not support writing");
                Console.WriteLine(e.Message);
            }
            catch(IOException e)
            {
                //Console.WriteLine("There was a failure while writing to the network");
                Console.WriteLine(e.Message);
            }            
        }

        /// <summary>
        /// 异步接收消息数据，不负责粘包处理
        /// </summary>
        /// <returns>返回接收到的字节数</returns>
        public async Task<int> ReadAsync()
        {
            try
            {
                if (IsFull())
                {
                    EnsureCapacity(m_IndexMask + 1);
                }

                int maxCount = m_Buffer.Length - 1 - m_Head;
                if (m_Tail > m_Head)
                    maxCount = Math.Min(maxCount, m_Tail - m_Head);
                
                int count = await m_Stream.ReadAsync(m_Buffer, m_Head, maxCount);
                m_Head = (m_Head + count) & m_IndexMask;
                return count;
            }
            catch (ObjectDisposedException e)
            {
                // The NetworkStream is closed
                return 0;
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine("The NetworkStream does not support reading");
                return 0;
            }
            catch(IOException e)
            {
                Console.WriteLine(e.Message);
                return 0;
            }
        }

        /// <summary>
        /// 上层解析完消息后需要调用此接口
        /// </summary>
        /// <param name="length"></param>
        public void FinishRead(int length)
        {
            m_Tail = (m_Tail + length) & m_IndexMask;
        }

        public void FetchBuffer(byte[] data)
        {

        }
    }
}