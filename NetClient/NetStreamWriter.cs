using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO;

namespace Framework.NetWork
{
    /// <summary>
    /// 负责网络数据发送，主线程同步接收数据，子线程异步发送数据
    /// 测试用例：
    /// 1、连接服务器失败   [PASS]
    /// 2、关闭服务器，再发送消息   []
    /// 3、客户端异常断开连接（参数错误、断电等）
    /// 4、断线重连
    /// 5、任何异常情况能否退出WriteAsync
    /// 6、主动断开连接
    /// 7、持续的发送协议时重复1-6
    /// </summary>
    sealed internal class NetStreamWriter : NetStream
    {
        private NetClientEx                 m_NetClient;
        private NetworkStream               m_Stream;
        private SemaphoreSlim               m_SendBufferSema;                       // 控制是否可以消息发送的信号量
                                                                                    // The count is decremented each time a thread enters the semaphore, and incremented each time a thread releases the semaphore
        private bool                        m_isSendingBuffer;                      // 发送消息IO是否进行中
        private bool                        m_QuitWriting;

        struct WriteCommand
        {
            public int Head;
            public int Fence;
        }
        private Queue<WriteCommand>         m_CommandQueue          = new Queue<WriteCommand>(8);

        internal NetStreamWriter(NetClientEx netClient, int capacity = 8 * 1024)
            : base(capacity)
        {
            if (netClient == null) throw new ArgumentNullException();

            m_NetClient = netClient;
        }

        internal void Start(NetworkStream stream)
        {
            m_Stream = stream;

            // setup environment
            m_Buffer.Clear();
            m_SendBufferSema?.Dispose();
            m_SendBufferSema = new SemaphoreSlim(0, 1);
            m_isSendingBuffer = false;
            m_QuitWriting = false;

            Task.Run(WriteAsync);
        }

        protected override void Dispose(bool disposing)
        {
            if (m_Disposed)
                return;

            if (disposing)
            {
                // free managed resources
            }

            // free unmanaged resources
            m_SendBufferSema?.Dispose();

            m_Disposed = true;
        }

        internal void Write(byte[] data, int offset, int length)
        {
            m_Buffer.Write(data, offset, length);
        }

        internal void Write(byte[] data)
        {
            m_Buffer.Write(data, 0, data.Length);
        }

        internal void FetchBufferToWrite(int length, out byte[] buf, out int offset)
        {
            m_Buffer.FetchBufferToWrite(length, out buf, out offset);
        }

        internal void FinishBufferWriting(int length)
        {
            m_Buffer.FinishBufferWriting(length);
        }

        internal void Close()
        {
            // release semaphore, make WriteAsync jump out of the while loop
            if (m_SendBufferSema.CurrentCount == 0)
            {
                m_QuitWriting = true;
                m_SendBufferSema.Release();
            }
        }

        internal void Flush()
        {
            if (m_NetClient?.state == ConnectState.Connected &&
                m_SendBufferSema != null &&
                m_SendBufferSema.CurrentCount == 0 &&           // The number of remaining threads that can enter the semaphore
                !m_isSendingBuffer &&                           // 上次消息已发送完成
                !m_Buffer.IsEmpty())                            // 已缓存一定的待发送消息
            {
                // cache the pending sending data
                m_CommandQueue.Enqueue(new WriteCommand() { Head = m_Buffer.Head, Fence = m_Buffer.Fence });

                // 每次push command完重置Fence
                m_Buffer.ResetFence();

                m_SendBufferSema.Release();                     // Sema.CurrentCount += 1
            }
        }

        private async void WriteAsync()
        {
            try
            {
                while (m_NetClient.state == ConnectState.Connected)
                {
                    await m_SendBufferSema.WaitAsync();         // CurrentCount==0将等待，直到Sema.CurrentCount > 0，执行完Sema.CurrentCount -= 1
                    if (m_QuitWriting)
                        break;
                    m_isSendingBuffer = true;
                    await FlushWrite();
                    m_isSendingBuffer = false;
                }
            }
            catch (SocketException e)
            {
                RaiseException(e);
            }
        }

        private async Task FlushWrite()
        {
            try
            {
                if (m_CommandQueue.Count == 0)
                    return;

                WriteCommand cmd = m_CommandQueue.Peek();

                int length = m_Buffer.GetUsedCapacity(cmd.Head);
                if (cmd.Head > m_Buffer.Tail)
                {
                    await m_Stream.WriteAsync(m_Buffer.Buffer, m_Buffer.Tail, length);
                }
                else
                {
                    if (cmd.Fence > 0)
                        await m_Stream.WriteAsync(m_Buffer.Buffer, m_Buffer.Tail, cmd.Fence - m_Buffer.Tail);
                    else
                        await m_Stream.WriteAsync(m_Buffer.Buffer, m_Buffer.Tail, m_Buffer.Buffer.Length - m_Buffer.Tail);

                    if (cmd.Head > 0)
                        await m_Stream.WriteAsync(m_Buffer.Buffer, 0, cmd.Head);
                }

                m_Buffer.FinishBufferSending(length);        // 数据发送完成，更新Tail
                m_CommandQueue.Dequeue();
            }
            catch (ObjectDisposedException e)
            {
                // The NetworkStream is closed
                RaiseException(e);
            }
            catch (ArgumentNullException e)
            {
                // The buffer parameter is NULL
                RaiseException(e);
            }
            catch (ArgumentOutOfRangeException e)
            {
                RaiseException(e);
            }
            catch (InvalidOperationException e)
            {
                RaiseException(e);
            }
            catch (IOException e)
            {
                RaiseException(e);
            }
        }

        private void RaiseException(Exception e)
        {
            m_NetClient.RaiseException(e);
        }
    }
}
