using System;
using System.Text;
using System.Threading.Tasks;
using Framework.NetWork.Log;

namespace Framework.NetWork
{
    public class NetManager<TMessage> where TMessage : class, IMessage
    {
        private NetClient m_NetClient;

        public NetManager()
        {
            Trace.EnableConsole();
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
            m_NetClient.FlushSending();
        }

        public void SetData(string context)
        {
            byte[] byteData = Encoding.ASCII.GetBytes(context);
            m_NetClient.Send(byteData);
            m_NetClient.FlushSending();
        }

        public string ReceiveData()
        {
            int offset;
            int length;
            ref readonly byte[] data = ref m_NetClient.BeginRead(out offset, out length);
            if (length == 0)
                return string.Empty;
            string context = Encoding.ASCII.GetString(data);
            m_NetClient.EndRead(length);
            return context;
        }
    }

    interface IMessageSerializer
    {
        ref readonly byte[] BeginRead(out int offset, out int length);
        void EndRead(int length);
    }

    interface IMessageDeserializer
    {
        int Deserializer(byte[] data, int offset, int length);
    }

    interface IMessageString : IMessageSerializer, IMessageDeserializer
    {
        void Write(string context);
        string Read();
    }
}
