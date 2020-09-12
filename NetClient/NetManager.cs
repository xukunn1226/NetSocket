﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.NetWork
{
    class NetManager
    {
        private NetClient m_NetClient;

        public void Init(string host, int port)
        {
            m_NetClient = new NetClient(host, port, OnConnected, OnDisconnected);
        }

        private void OnConnected()
        {

        }

        private void OnDisconnected(int ret)
        {

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
}
