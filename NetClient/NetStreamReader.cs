using System;
using System.IO;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace Framework.NetWork
{
    sealed internal class NetStreamReader : NetStream
    {
        private NetClientEx         m_NetClient;
        private NetworkStream       m_Stream;
        private int                 m_ReceiveByte;

        internal NetStreamReader(NetClientEx netClient, int capacity = 8 * 1024)
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
            
            Task.Run(ReceiveAsync);
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

            m_Disposed = true;
        }

        internal ref readonly byte[] FetchBufferToRead(out int offset, out int length)
        {
            return ref m_Buffer.FetchBufferToRead(out offset, out length);
        }

        internal void FinishRead(int length)
        {
            m_Buffer.FinishRead(length);
        }

        private async void ReceiveAsync()
        {
            try
            {
                while (m_NetClient.state == ConnectState.Connected)
                {
                    m_ReceiveByte = await ReadAsync();
                    if (m_ReceiveByte <= 0)              // 连接中断
                    {
                        RaiseException(new Exception("socket disconnected"));
                    }
                }
            }
            catch (SocketException e)
            {
                RaiseException(e);
            }
        }

        /// <summary>
        /// 异步接收消息数据
        /// </summary>
        /// <returns>返回接收到的字节数</returns>
        internal async Task<int> ReadAsync()
        {
            try
            {
                if (!m_Stream.CanRead)
                {
                    return 0;
                }

                int maxCount = m_Buffer.GetContinuousFreeCapacityToEnd();        // 获得连续的空闲buf

                int count = await m_Stream.ReadAsync(m_Buffer.Buffer, m_Buffer.Head, maxCount);
                m_Buffer.Head = (m_Buffer.Head + count) & m_Buffer.IndexMask;
                return count;
            }
            catch (ObjectDisposedException e)
            {
                //Trace.Error(e.ToString());          // The NetworkStream is closed
                return 0;
            }
            catch (InvalidOperationException e)
            {
                //Trace.Error(e.ToString());          // The NetworkStream does not support reading
                return 0;
            }
            catch (IOException e)
            {
                //Trace.Error(e.ToString());
                return 0;
            }
        }

        private void RaiseException(Exception e)
        {
            m_NetClient.RaiseException(e);
        }
    }
}
