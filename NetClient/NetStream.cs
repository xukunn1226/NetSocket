using System;
using System.Threading;

namespace Framework.NetWork
{
    abstract internal class NetStream : IDisposable
    {
        protected bool                      m_Disposed;
        protected NetRingBuffer             m_Buffer;

        internal NetStream(int capacity = 8 * 1024)
        {
            m_Buffer = new NetRingBuffer(capacity);
        }

        ~NetStream()
        {
            Dispose(false);
        }

        protected abstract void Dispose(bool disposing);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
