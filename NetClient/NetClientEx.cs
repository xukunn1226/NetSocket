using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using Framework.NetWork.Log;

namespace Framework.NetWork
{
    public class NetClientEx
    {
        public delegate void onConnected();
        public delegate void onDisconnected(int ret);

        public ConnectState         state { get; private set; } = ConnectState.Disconnected;

        private TcpClient           m_Client;

        private string              m_Host;
        private int                 m_Port;

        private onConnected         m_ConnectedHandler;
        private onDisconnected      m_DisconnectedHandler;

        private NetStreamWriter     m_StreamWriter;

        public NetClientEx(string host, int port, onConnected connectionHandler = null, onDisconnected disconnectedHandler = null)
        {
            m_ConnectedHandler = connectionHandler;
            m_DisconnectedHandler = disconnectedHandler;

            m_StreamWriter = new NetStreamWriter(this, 4 * 1024);

            m_Host = host;
            m_Port = port;
        }

        async public Task Connect()
        {
            m_Client = new TcpClient();
            m_Client.NoDelay = true;

            try
            {
                IPAddress ip = IPAddress.Parse(m_Host);
                state = ConnectState.Connecting;
                await m_Client.ConnectAsync(ip, m_Port);
                OnConnected();

                m_StreamWriter.Start(m_Client.GetStream());
            }
            catch (ArgumentNullException e)
            {
                Trace.Debug(e.ToString());
                RaiseException(e);
            }
            catch (ArgumentOutOfRangeException e)
            {
                Trace.Debug(e.ToString());
                RaiseException(e);
            }
            catch (ObjectDisposedException e)
            {
                Trace.Debug(e.ToString());
                RaiseException(e);
            }
            catch (SocketException e)
            {
                Trace.Debug(e.ToString());
                RaiseException(e);
            }
        }

        async public Task Reconnect()
        {
            await Connect();
        }

        private void OnConnected()
        {
            state = ConnectState.Connected;
            m_ConnectedHandler?.Invoke();
        }

        internal void OnDisconnected(int ret)
        {
            state = ConnectState.Disconnected;
            m_DisconnectedHandler?.Invoke(ret);
        }

        internal void RaiseException(Exception e)
        {
            Trace.Debug(e.ToString());
            InternalClose();
        }

        public void Close()
        {
            try
            {
                InternalClose();
            }
            catch (Exception e)
            {
                Trace.Debug(e.ToString());
            }
        }

        private void InternalClose()
        {
            //if (m_SendBufferSema != null)
            //{
            //    // release semaphore, make WriteAsync jump out of the while loop
            //    if (m_SendBufferSema.CurrentCount == 0)
            //    {
            //        m_QuitWriteOp = true;                       // trigger quitting WriteAsync loop
            //        m_SendBufferSema.Release();
            //    }

            //    m_SendBufferSema.Dispose();
            //    m_SendBufferSema = null;
            //}

            if(m_StreamWriter != null)
            {

            }

            if (m_Client != null)
            {
                if (m_Client.Connected)                          // 当远端主动断开网络时，NetworkStream呈已关闭状态
                    m_Client.GetStream().Close();
                m_Client.Close();
                m_Client = null;

                OnDisconnected(0);
            }
        }

        public void Send(byte[] buf, int offset, int length)
        {
            try
            {
                m_StreamWriter.Write(buf, offset, length);
                m_StreamWriter.Flush();
            }
            catch (ArgumentNullException e)
            {
                RaiseException(e);
            }
            catch (ArgumentOutOfRangeException e)
            {
                RaiseException(e);
            }
        }

        public void Send(byte[] buf)
        {
            Send(buf, 0, buf.Length);
        }


        // https://docs.microsoft.com/zh-cn/dotnet/api/system.net.sockets.socket.connected?redirectedfrom=MSDN&view=netcore-3.1#System_Net_Sockets_Socket_Connected
        public bool IsConnected()
        {
            if (m_Client == null || m_Client.Client == null)
                throw new ArgumentNullException("socket");

            if (!m_Client.Client.Connected)      // Connected记录的是最近一次Send或Receive时的状态
                return false;

            bool isConnected = true;
            bool blockingState = m_Client.Client.Blocking;
            try
            {
                byte[] tmp = new byte[1];

                m_Client.Client.Blocking = false;
                m_Client.Client.Send(tmp, 0, 0);
                Trace.Debug("Connected!");
                isConnected = true;
            }
            catch (SocketException e)
            {
                // 10035 == WSAEWOULDBLOCK
                if (e.NativeErrorCode.Equals(10035))
                {
                    Trace.Debug("Still Connected, but the Send would block");
                    isConnected = true;
                }
                else
                {
                    Trace.Debug($"Disconnected: error code {e.NativeErrorCode}!");
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
        public bool IsConnected2()
        {
            if (m_Client == null || m_Client.Client == null)
                throw new ArgumentNullException("socket");

            if (m_Client.Client.Poll(10, SelectMode.SelectRead) && (m_Client.Client.Available == 0) || !m_Client.Client.Connected)
                return false;
            else
                return true;
        }
    }
}