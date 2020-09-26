using System;
using System.IO;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace Framework.NetWork
{
    sealed internal class NetStreamReader : NetStream
    {
        private NetClient           m_NetClient;
        private NetworkStream       m_Stream;

        internal NetStreamReader(NetClient netClient, int capacity = 8 * 1024)
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
            return ref m_Buffer.Read(out offset, out length);
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
                    int freeCount = m_Buffer.GetConsecutiveUnusedCapacity();             // 填充连续的空闲空间                
                    if (freeCount == 0)
                        throw new ArgumentOutOfRangeException("ReadAsync: buff is full");

                    int receiveByte = await m_Stream.ReadAsync(m_Buffer.Buffer, m_Buffer.Head, freeCount);
                    m_Buffer.Head = (m_Buffer.Head + receiveByte) & m_Buffer.IndexMask;

                    if (receiveByte <= 0)              // 连接中断
                    {
                        RaiseException(new Exception("socket disconnected"));
                    }
                }
            }
            catch (SocketException e)
            {
                RaiseException(e);
            }
            catch (ObjectDisposedException e)
            {
                //Trace.Error(e.ToString());          // The NetworkStream is closed
                RaiseException(e);
            }
            catch (InvalidOperationException e)
            {
                //Trace.Error(e.ToString());          // The NetworkStream does not support reading
                RaiseException(e);
            }
            catch (IOException e)
            {
                //Trace.Error(e.ToString());
                RaiseException(e);
            }
            catch (ArgumentOutOfRangeException e)
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
