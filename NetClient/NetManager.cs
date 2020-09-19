using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Framework.NetWork.Log;

namespace Framework.NetWork
{
    public class NetManager<TMessage> where TMessage : class
    {
        private NetClient               m_NetClient;
        private IPacket<TMessage>     m_Parser;
        private List<TMessage>          m_MessageList = new List<TMessage>();

        protected NetManager() { }

        public NetManager(IPacket<TMessage> parser)
        {
            Trace.EnableConsole();
            m_Parser = parser;
        }

        async public Task Connect(string host, int port)
        {
            m_NetClient = new NetClient(host, port, OnConnected, OnDisconnected);
            await m_NetClient.Connect();
        }

        async public Task Reconnect()
        {
            if (m_NetClient == null)
                throw new ArgumentNullException();
            await m_NetClient.Reconnect();
        }

        public void Close()
        {
            if (m_NetClient == null)
                throw new ArgumentNullException();
            m_NetClient.Close();
        }

        public ConnectState state { get { return m_NetClient?.state ?? ConnectState.Disconnected; } }

        private void OnConnected()
        {
            Trace.Debug("Connected...");
        }

        private void OnDisconnected(int ret)
        {
            Trace.Debug("...Disconnected");
        }

        public void Update()
        {
            if (m_NetClient == null)
                return;

            m_NetClient.FlushWrite();
            ReceiveData();
        }

        public void SetData(TMessage data, bool needFlush = false)
        {
            byte[] buf = m_Parser.Serialize(data);
            m_NetClient.Send(buf);
            if(needFlush)
                m_NetClient.FlushWrite();
        }

        private void ReceiveData()
        {
            int offset;
            int length;     // 已接收的消息长度
            ref readonly byte[] data = ref m_NetClient.BeginRead(out offset, out length);
            if (length == 0)
                return;

            int totalRealLength = 0;            // 实际解析的总长度(byte)
            int startOffset = offset;
            int totalLength = length;
            m_MessageList.Clear();
            while (true)
            {
                startOffset += totalRealLength;
                totalLength -= totalRealLength;

                int realLength;                 // 单次解析的长度(byte)
                TMessage msg;
                bool success = m_Parser.Deserialize(in data, startOffset, totalLength, out realLength, out msg);
                if (success)
                    m_MessageList.Add(msg);

                totalRealLength += realLength;
                if (!success || totalRealLength == totalLength)
                    break;                      // 解析失败或者已接收的消息长度解析完了
            }
            m_NetClient.EndRead(totalRealLength);

            // dispatch
            foreach(var msg in m_MessageList)
            {
                //Dispatch(msg);
                Trace.Debug($"Receive:=== {msg}");
            }
        }
    }
}
