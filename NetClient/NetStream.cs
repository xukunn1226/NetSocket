using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.NetWork
{
    abstract public class NetStream : NetRingBuffer, IDisposable
    {
        private bool m_Disposed;

        public NetStream(int capacity = 8 * 1024) : base(capacity)
        { }

        ~NetStream()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (m_Disposed)
                return;

            if (disposing)
            {
                // free managed resources
            }

            // free unmanaged resources

            m_Disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
