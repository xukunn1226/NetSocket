using System.Collections;
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
        private const int       m_DefaultCapacity = 1024;
        private NetworkStream   m_Stream;
        private NetClient       m_NetClient;

        private byte[]          m_Buffer;
        private int             m_Head;
        private int             m_Tail;
        private int             m_IndexMask;

        public byte[]           Buffer  { get { return m_Buffer; } }

        public int              Head    { get { return m_Head; } }

        public int              Tail    { get { return m_Tail; } }

        public NetStreamBuffer(NetClient netClient, int capacity = 8 * 1024)
        {
            if (netClient == null) throw new ArgumentNullException();

            m_NetClient = netClient;
            EnsureCapacity(capacity);
        }

        public void SetStream(NetworkStream stream)
        {
            m_Stream = stream;
            Clear();
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

        private int GetMaxCapacity()
        {
            return m_Buffer.Length - 1;
        }

        private int GetFreeCapacity()
        {
            return GetMaxCapacity() - GetUsedCapacity();
        }

        private int GetUsedCapacity()
        {
            return m_Head >= m_Tail ? m_Head - m_Tail : m_Buffer.Length - (m_Tail - m_Head);
        }

        public bool Write(byte[] data, int offset, int length)
        {
            if (data == null)
                throw new ArgumentNullException("data == null");

            // 传入参数的合法性检查:可写入空间大小的检查
            if (offset + length > data.Length)
                throw new ArgumentOutOfRangeException("offset + length > data.Length");

            // expand buffer
            while(length > GetFreeCapacity())
            {
                EnsureCapacity(GetMaxCapacity() + 1);
            }

            if(m_Head + length <= m_Buffer.Length)
            {
                System.Buffer.BlockCopy(data, offset, m_Buffer, m_Head, length);
            }
            else
            {
                int countToEnd = m_Buffer.Length - m_Head;
                System.Buffer.BlockCopy(data, offset, m_Buffer, m_Head, countToEnd);
                System.Buffer.BlockCopy(data, countToEnd, m_Buffer, 0, length - countToEnd);
            }
            m_Head = (m_Head + length) & m_IndexMask;

            return true;
        }

        public bool Write(byte[] data)
        {
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
            if(m_Buffer == null || m_Buffer.Length < min)
            {
                int newCapacity = m_Buffer == null || m_Buffer.Length == 0 ? m_DefaultCapacity : m_Buffer.Length * 2;
                if((uint)newCapacity > Int32.MaxValue)
                    newCapacity = Int32.MaxValue;
                if(newCapacity < min)
                    newCapacity = min;
                newCapacity = NextPowerOfTwo(newCapacity);

                // expand buffer
                byte[] newBuf = new byte[newCapacity];
                if(m_Head > m_Tail)
                {
                    System.Buffer.BlockCopy(m_Buffer, m_Tail, newBuf, m_Tail, m_Head - m_Tail);
                    //m_Tail = m_Tail;      // no change
                    //m_Head = m_Head;      // no change
                }
                else if(m_Head < m_Tail)
                {
                    int countToEnd = m_Buffer.Length - m_Tail;
                    System.Buffer.BlockCopy(m_Buffer, m_Tail, newBuf, newBuf.Length - countToEnd, countToEnd);

                    if(m_Head > 0)
                        System.Buffer.BlockCopy(m_Buffer, 0, newBuf, 0, m_Head);

                    m_Tail = newBuf.Length - countToEnd;
                    //m_Head = m_Head;      // no change
                }
                m_Buffer = newBuf;
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
                    await m_Stream.WriteAsync(m_Buffer, m_Tail, m_Buffer.Length - m_Tail);
                    if(m_Head > 0)
                        await m_Stream.WriteAsync(m_Buffer, 0, m_Head);
                }

                m_Tail = (m_Tail + count) & m_IndexMask;        // 数据发送完成，更新Tail
            }
            catch (ObjectDisposedException e)
            {
                Console.WriteLine(e.Message);       // The NetworkStream is closed
                m_NetClient.OnDisconnected(-1);
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine(e.Message);       // The buffer parameter is NULL
                m_NetClient.OnDisconnected(-1);
            }
            catch(ArgumentOutOfRangeException e)
            {
                Console.WriteLine(e.Message);
                m_NetClient.OnDisconnected(-1);
            }
            catch(InvalidOperationException e)
            {
                Console.WriteLine(e.Message);       // The NetworkStream does not support writing
                m_NetClient.OnDisconnected(-1);
            }
            catch(IOException e)
            {
                Console.WriteLine(e.Message);       // There was a failure while writing to the network
                m_NetClient.OnDisconnected(-1);
            }
        }

        /// <summary>
        /// 异步接收消息数据
        /// </summary>
        /// <returns>返回接收到的字节数</returns>
        public async Task<int> ReadAsync()
        {
            try
            {
                if (!m_Stream.CanRead)
                {
                    Console.WriteLine("ReadAsync: Can't read");
                    return 0;
                }

                if (IsFull())
                {
                    EnsureCapacity(m_Buffer.Length + 1);
                }

                int maxCount = GetFreeCapacity();                               // 最大填充容量
                maxCount = Math.Min(maxCount, m_Buffer.Length - m_Head);        // 一次最多填充至尾端
                
                int count = await m_Stream.ReadAsync(m_Buffer, m_Head, maxCount);
                m_Head = (m_Head + count) & m_IndexMask;
                return count;
            }
            catch (ObjectDisposedException e)
            {
                Console.WriteLine(e.ToString());          // The NetworkStream is closed
                return 0;
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine(e.ToString());          // The NetworkStream does not support reading
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

        public void FetchBuffer(ref byte[] data, out int offset, out int length)
        {
            data = m_Buffer;
            offset = m_Tail;
            length = GetUsedCapacity();
        }
    }
}