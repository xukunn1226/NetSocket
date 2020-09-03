using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace Framework.NetWork
{
    public class NetClient
    {
        public delegate void        onConnected(NetClient client);
        public delegate void        onDisconnected(NetClient client, int ret);

        private TcpClient           m_Client;

        private IPAddress           m_IP;
        private int                 m_Port;

        private onConnected         m_ConnectedHandler;
        private onDisconnected      m_DisconnectedHandler;

        private NetStreamBuffer     m_SendBuffer;                                                   // 消息发送缓存池
        private NetStreamBuffer     m_ReceiveBuffer;                                                // 消息接收缓存池

        private SemaphoreSlim       m_SendBufferSema = new SemaphoreSlim(0, 1);                     // 控制消息发送的信号量

        public NetClient(string host, int port, onConnected connectionHandler, onDisconnected disconnectedHandler)
        {
            m_ConnectedHandler = connectionHandler;
            m_DisconnectedHandler = disconnectedHandler;

            Connect(host, port);
        }

        async void Connect(string host, int port)
        {
            m_Client = new TcpClient();
            m_Client.NoDelay = true;
            // m_Client.ReceiveBufferSize = 8192;
            // m_Client.SendBufferSize = 8192;

            try
            {
                m_IP = IPAddress.Parse(host);
                m_Port = port;
                await m_Client.ConnectAsync(m_IP, m_Port);

                m_ConnectedHandler?.Invoke(this);
            }
            catch(ArgumentNullException e)
            {
                m_DisconnectedHandler?.Invoke(null, -1);
                //Debug.LogError($"Client::Connect {e.Message}");
            }
            catch(ArgumentOutOfRangeException e)
            {
                m_DisconnectedHandler?.Invoke(null, -2);
            }
            catch(ObjectDisposedException e)
            {
                m_DisconnectedHandler?.Invoke(null, -3);
            }
            catch(SocketException e)
            {
                m_DisconnectedHandler?.Invoke(null, -4);
                //Debug.LogError($"Client::Connect {e.Message}");
            }

            m_SendBuffer = new NetStreamBuffer(m_Client.GetStream(), 4 * 1024);
            m_ReceiveBuffer = new NetStreamBuffer(m_Client.GetStream(), 8 * 1024);

            FlushOutputStream();
            ReceiveAsync();
        }

        private void OnConnected()
        {

        }

        private void OnDisconnected(int ret)
        {

        }

        public void Close()
        {
            try
            {
                m_Client.GetStream().Close();
                m_Client.Close();

                m_DisconnectedHandler?.Invoke(this, 0);
            }
            catch(Exception e)
            {
                //Debug.LogError($"Client::Close {e.Message}");
            }
        }

        private void LateUpdate()
        {
            // 一帧触发一次消息发送
            if (m_SendBufferSema.CurrentCount == 0 && !m_SendBuffer.IsEmpty())             // The number of remaining threads that can enter the semaphore
            {
                m_SendBufferSema.Release();                     // Sema.CurrentCount += 1
            }

            // 解析消息
            ParseMsg();
        }

        private async void FlushOutputStream()
        {
            try
            {
                while (m_Client.Connected)
                {
                    await m_SendBufferSema.WaitAsync();         // 等待Sema.CurrentCount > 0，执行完Sema.CurrentCount -= 1
                    await m_SendBuffer.FlushWrite();
                }
            }
            catch(SocketException e)
            {
                //Debug.LogError($"FlushOutputStream  {e.Message}");
                m_DisconnectedHandler?.Invoke(this, e.ErrorCode);         // 异常断开
            }
        }

        public void Send(byte[] buf, int offset, int length)
        {
            if (buf == null || offset + length > buf.Length)
            {
                throw new ArgumentException("Send: offset + length > buf.Length");
            }

            m_SendBuffer.Write(buf, offset, length);
        }

        public void Send(byte[] buf)
        {
            if (buf == null) throw new ArgumentNullException("Send...");
            Send(buf, 0, buf.Length);
        }






        private async void ReceiveAsync()
        {
            try
            {
                //while(m_Client.Connected)
                while(true)
                {
                    int count = await m_ReceiveBuffer.ReadAsync();
                    if(count == 0)
                    {
                        m_DisconnectedHandler?.Invoke(this, 2);     // 远端主动断开网络
                    }
                }
            }
            catch(SocketException e)
            {
                //Debug.LogError($"ReceiveAsync   {e.Message}");
                m_DisconnectedHandler?.Invoke(this, e.ErrorCode);
            }
        }

        private void ParseMsg()
        {

        }

        // https://docs.microsoft.com/zh-cn/dotnet/api/system.net.sockets.socket.connected?redirectedfrom=MSDN&view=netcore-3.1#System_Net_Sockets_Socket_Connected
        public bool IsConnected(Socket socket)
        {
            if (socket == null)
                throw new ArgumentNullException("socket");

            if (!socket.Connected)      // Connected记录的是最近一次Send或Receive时的状态
                return false;

            bool isConnected = true;
            bool blockingState = m_Client.Client.Blocking;
            try
            {
                byte[] tmp = new byte[1];

                m_Client.Client.Blocking = false;
                m_Client.Client.Send(tmp, 0, 0);
                Console.WriteLine("Connected!");
                isConnected = true;
            }
            catch (SocketException e)
            {
                // 10035 == WSAEWOULDBLOCK
                if (e.NativeErrorCode.Equals(10035))
                {
                    Console.WriteLine("Still Connected, but the Send would block");
                    isConnected = true;
                }
                else
                {
                    Console.WriteLine("Disconnected: error code {0}!", e.NativeErrorCode);
                    isConnected = false;
                }
            }
            finally
            {
                m_Client.Client.Blocking = blockingState;
            }
            return isConnected;
        }

        // 适用于对端正常关闭socket下的本地socket状态检测，在非正常关闭如断电、拔网线的情况下不起作用
        public bool IsConnected2(Socket socket)
        {
            if (socket.Poll(10, SelectMode.SelectRead) && (socket.Available == 0) || !socket.Connected)
                return false;
            else
                return true;
        }
    }
}