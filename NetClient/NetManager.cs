using System;
using System.Text;
using System.Threading.Tasks;
using Framework.NetWork.Log;

namespace Framework.NetWork
{
    public class NetManager<TMessage> where TMessage : class
    {
        private NetClient           m_NetClient;
        private IProtocol<TMessage> m_Parser;

        protected NetManager() { }

        public NetManager(IProtocol<TMessage> parser)
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

        public void Tick()
        {
            if (m_NetClient == null)
                return;

            m_NetClient.Flush();
        }

        public void SetData(TMessage data, bool needFlush = false)
        {
            byte[] buf = m_Parser.Serialize(data);
            m_NetClient.Send(buf);
            if(needFlush)
                m_NetClient.Flush();
        }

        public TMessage ReceiveData()
        {
            int offset;
            int length;
            ref readonly byte[] data = ref m_NetClient.BeginRead(out offset, out length);
            if (length == 0)
                return default(TMessage);

            int realLength;
            TMessage msg = m_Parser.Deserialize(in data, offset, length, out realLength);
            m_NetClient.EndRead(realLength);
            return msg;
        }
    }
}
