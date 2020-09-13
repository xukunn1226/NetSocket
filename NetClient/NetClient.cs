using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace Framework.NetWork
{
    public enum ConnectState
    {
        Disconnected,
        Connecting,
        Connected,
    }

    public class NetClient
    {
        public delegate void        onConnected();
        public delegate void        onDisconnected(int ret);

        
        private ConnectState        m_State = ConnectState.Disconnected;
        public ConnectState         state { get { return m_State; } }

        private TcpClient           m_Client;

        private string              m_Host;
        private int                 m_Port;

        private onConnected         m_ConnectedHandler;
        private onDisconnected      m_DisconnectedHandler;

        private NetStreamBuffer     m_SendBuffer;                                                   // 消息发送缓存池
        private NetStreamBuffer     m_ReceiveBuffer;                                                // 消息接收缓存池

        private SemaphoreSlim       m_SendBufferSema;                                               // 控制是否可以消息发送的信号量
                                                                                                    // The count is decremented each time a thread enters the semaphore, and incremented each time a thread releases the semaphore
        private bool                m_isSendingBuffer;                                              // 发送消息IO是否进行中

        private int                 m_ReceiveByte;

        public NetClient(string host, int port, onConnected connectionHandler = null, onDisconnected disconnectedHandler = null)
        {
            m_ConnectedHandler = connectionHandler;
            m_DisconnectedHandler = disconnectedHandler;

            m_SendBuffer = new NetStreamBuffer(this, 4 * 1024);
            m_ReceiveBuffer = new NetStreamBuffer(this, 8 * 1024);

            m_Host = host;
            m_Port = port;
        }

        async public Task Connect()
        {
            m_Client = new TcpClient();
            m_Client.NoDelay = true;

            m_SendBufferSema = new SemaphoreSlim(0, 1);
            m_isSendingBuffer = false;

            try
            {
                IPAddress ip = IPAddress.Parse(m_Host);
                m_State = ConnectState.Connecting;
                await m_Client.ConnectAsync(ip, m_Port);                
                OnConnected();

                m_SendBuffer.SetStream(m_Client.GetStream());
                m_ReceiveBuffer.SetStream(m_Client.GetStream());

                WriteAsync();
                ReceiveAsync();
            }
            catch(ArgumentNullException e)
            {
                Console.WriteLine(e.ToString());
                OnDisconnected(-1);
            }
            catch(ArgumentOutOfRangeException e)
            {
                Console.WriteLine(e.ToString());
                OnDisconnected(-2);
            }
            catch(ObjectDisposedException e)
            {
                Console.WriteLine(e.ToString());
                OnDisconnected(-3);
            }
            catch(SocketException e)
            {
                Console.WriteLine(e.ToString());
                OnDisconnected(-4);
            }
        }

        async public Task Reconnect()
        {
            await Connect();
        }

        private void OnConnected()
        {
            m_State = ConnectState.Connected;
            m_ConnectedHandler?.Invoke();
        }

        internal void OnDisconnected(int ret)
        {
            // release semaphore, then WriteAsync jump out of the while loop
            if(m_SendBufferSema.CurrentCount == 0)
                m_SendBufferSema.Release();

            m_State = ConnectState.Disconnected;
            m_DisconnectedHandler?.Invoke(ret);
        }

        public void Close()
        {
            try
            {
                m_Client.GetStream().Close();
                m_Client.Close();

                OnDisconnected(0);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void FlushSending()
        {
            if (m_State == ConnectState.Connected && 
                m_SendBufferSema != null &&
                m_SendBufferSema.CurrentCount == 0 &&           // The number of remaining threads that can enter the semaphore
                !m_isSendingBuffer &&                           // 上次消息已发送完成
                !m_SendBuffer.IsEmpty())                        // 已缓存一定的待发送消息
            {
                m_SendBufferSema.Release();                     // Sema.CurrentCount += 1
            }
        }

        private async void WriteAsync()
        {
            try
            {
                while(m_State == ConnectState.Connected)
                {
                    await m_SendBufferSema.WaitAsync();         // CurrentCount==0将等待，直到Sema.CurrentCount > 0，执行完Sema.CurrentCount -= 1
                    m_isSendingBuffer = true;
                    await m_SendBuffer.FlushWrite();
                    m_isSendingBuffer = false;
                }
            }
            catch(SocketException e)
            {
                Console.WriteLine(e.ToString());
                OnDisconnected(-1);
            }
        }

        public void Send(byte[] buf, int offset, int length)
        {
            try
            {
                m_SendBuffer.Write(buf, offset, length);
            }
            catch(ArgumentNullException e)
            {
                Console.WriteLine(e.ToString());
                OnDisconnected(-1);
            }
            catch(ArgumentOutOfRangeException e)
            {
                Console.WriteLine(e.ToString());
                OnDisconnected(-1);
            }
        }

        public void Send(byte[] buf)
        {
            Send(buf, 0, buf.Length);
        }

        public ref readonly byte[] BeginRead(out int offset, out int length)
        {
            return ref m_ReceiveBuffer.Fetch(out offset, out length);
        }

        public void EndRead(int length)
        {
            m_ReceiveBuffer.FinishRead(length);
        }

        private async void ReceiveAsync()
        {
            try
            {
                while(m_State == ConnectState.Connected)
                {
                    m_ReceiveByte = await m_ReceiveBuffer.ReadAsync();
                    if(m_ReceiveByte <= 0)
                    {
                        OnDisconnected(0);              // 远端主动断开网络
                    }
                }
            }
            catch(SocketException e)
            {
                Console.WriteLine(e.ToString());
                OnDisconnected(-1);
            }
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