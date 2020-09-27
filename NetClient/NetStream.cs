using System;

namespace Framework.NetWork
{
    abstract internal class NetStream : NetRingBuffer, IDisposable
    {
        protected bool                      m_Disposed;
        //protected NetRingBuffer             m_Buffer;

        internal NetStream(int capacity = 8 * 1024)
            : base(capacity)
        {
            //m_Buffer = new NetRingBuffer(capacity);
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
