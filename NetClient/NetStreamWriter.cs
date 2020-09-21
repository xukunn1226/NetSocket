using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO;

namespace Framework.NetWork
{
    public class NetStreamWriter : NetStream
    {
        private NetRingBuffer     m_Buffer;
        private NetworkStream     m_Stream;
        private NetClient         m_NetClient;

        public NetStreamWriter(NetClient netClient, int capacity = 8 * 1024)
        {
            if (netClient == null) throw new ArgumentNullException();

            m_NetClient = netClient;
            m_Buffer = new NetRingBuffer(capacity);
        }

        public void SetStream(NetworkStream stream)
        {
            m_Stream = stream;
            m_Buffer.Clear();
        }

        public void Write(byte[] data, int offset, int length)
        {
            m_Buffer.Write(data, offset, length);
        }

        public void Write(byte[] data)
        {
            m_Buffer.Write(data, 0, data.Length);
        }

        public bool FetchBufferToWrite(int length, out byte[] buf, out int offset)
        {
            return m_Buffer.FetchBufferToWrite(length, out buf, out offset);
        }

        /// <summary>
        /// 异步发送Buff所有数据，由上层决定什么时候发送（最佳实践：一帧调用一次）
        /// </summary>
        /// <returns></returns>
        internal async Task FlushWrite(int head)
        {
            try
            {
                if (m_Buffer.IsEmpty() || !m_Stream.CanWrite)
                    return;

                int length = m_Buffer.GetUsedCapacity(head);       // m_Head可能发生race condition
                if (head > m_Buffer.Tail)
                {
                    await m_Stream.WriteAsync(m_Buffer.Buffer, m_Buffer.Tail, length);
                }
                else
                {
                    if (m_Buffer.Fence > 0)
                        await m_Stream.WriteAsync(m_Buffer.Buffer, m_Buffer.Tail, m_Buffer.Fence - m_Buffer.Tail);
                    else
                        await m_Stream.WriteAsync(m_Buffer.Buffer, m_Buffer.Tail, m_Buffer.Buffer.Length - m_Buffer.Tail);

                    if (m_Buffer.Head > 0)
                        await m_Stream.WriteAsync(m_Buffer.Buffer, 0, m_Buffer.Head);
                }

                m_Buffer.FinishBufferSending(length);        // 数据发送完成，更新Tail
            }
            catch (ObjectDisposedException e)
            {
                // The NetworkStream is closed
                m_NetClient.RaiseException(e);
            }
            catch (ArgumentNullException e)
            {
                // The buffer parameter is NULL
                m_NetClient.RaiseException(e);
            }
            catch (ArgumentOutOfRangeException e)
            {
                m_NetClient.RaiseException(e);
            }
            catch (InvalidOperationException e)
            {
                m_NetClient.RaiseException(e);
            }
            catch (IOException e)
            {
                m_NetClient.RaiseException(e);
            }
        }
    }
}
