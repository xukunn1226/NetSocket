using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.NetWork
{
    internal class RingBufferStream
    {
        private const int       m_DefaultCapacity   = 1024;
        private byte[]          m_Buffer;
        private int             m_Head;
        private int             m_Tail;
        private int             m_Fence             = -1;
        private int             m_IndexMask;

        internal RingBufferStream(int capacity = 8 * 1024)
        {
            EnsureCapacity(capacity);
        }

        private void Clear()
        {
            m_Head = 0;
            m_Tail = 0;
            m_Fence = -1;
        }

        internal bool IsEmpty()
        {
            return m_Head == m_Tail;
        }

        internal bool IsFull()
        {
            return ((m_Head + 1) & m_IndexMask) == m_Tail;
        }

        private int GetMaxCapacity()
        {
            return m_Buffer.Length - 1;
        }

        private int GetFreeCapacity()
        {
            return GetMaxCapacity() - GetUsedCapacity();
        }

        private int GetUsedCapacity()
        {
            return m_Head >= m_Tail ? m_Head - m_Tail : m_Buffer.Length - (m_Tail - m_Head);
        }

        private int NextPowerOfTwo(int n)
        {
            n--;
            n |= n >> 1;
            n |= n >> 2;
            n |= n >> 4;
            n |= n >> 8;
            n |= n >> 16;
            n++;
            return n;
        }

        private void EnsureCapacity(int min)
        {
            if (m_Buffer == null || m_Buffer.Length < min)
            {
                int newCapacity = m_Buffer == null || m_Buffer.Length == 0 ? m_DefaultCapacity : m_Buffer.Length * 2;
                if ((uint)newCapacity > Int32.MaxValue)
                    newCapacity = Int32.MaxValue;
                if (newCapacity < min)
                    newCapacity = min;
                newCapacity = NextPowerOfTwo(newCapacity);

                // expand buffer
                byte[] newBuf = new byte[newCapacity];
                if (m_Head > m_Tail)
                {
                    System.Buffer.BlockCopy(m_Buffer, m_Tail, newBuf, m_Tail, m_Head - m_Tail);
                    //m_Tail = m_Tail;      // no change
                    //m_Head = m_Head;      // no change
                }
                else if (m_Head < m_Tail)
                {
                    int countToEnd = m_Buffer.Length - m_Tail;
                    System.Buffer.BlockCopy(m_Buffer, m_Tail, newBuf, newBuf.Length - countToEnd, countToEnd);

                    if (m_Head > 0)
                        System.Buffer.BlockCopy(m_Buffer, 0, newBuf, 0, m_Head);

                    m_Tail = newBuf.Length - countToEnd;
                    //m_Head = m_Head;      // no change
                    if (m_Fence > 0)
                    {
                        if (m_Fence < m_Tail)
                            throw new ArgumentException($"m_Fence{m_Fence} < m_Tail{m_Tail}");
                        m_Fence = newBuf.Length - (m_Buffer.Length - m_Fence);
                    }
                }
                m_Buffer = newBuf;
                m_IndexMask = m_Buffer.Length - 1;
            }
        }

        // get continous free capacity from head to buffer end
        private int GetContinuousFreeCapacityToEnd()
        {
            return Math.Min(GetFreeCapacity(), m_Head >= m_Tail ? m_Buffer.Length - m_Head : 0);
        }

        // get continous free capacity from buffer start to tail
        private int GetContinuousFreeCapacityFromStart()
        {
            int count = 0;
            if (m_Head < m_Tail)
            {
                count = m_Tail - m_Head;
            }
            else if (m_Head > m_Tail)
            {
                count = m_Tail;
            }
            else
            {
                count = m_Buffer.Length - m_Head;
            }

            return Math.Min(GetFreeCapacity(), count);
        }

        private int GetContinuousUsedCapacity()
        {
            return m_Head >= m_Tail ? m_Head - m_Tail : m_Buffer.Length - m_Tail;
        }

        internal ref readonly byte[] FetchBufferToRead(out int offset, out int length)
        {
            offset = m_Tail;
            length = GetContinuousUsedCapacity();
            return ref m_Buffer;
        }

        internal void FinishRead(int length)
        {
            m_Tail = (m_Tail + length) & m_IndexMask;
        }

        internal bool Write(byte[] data, int offset, int length)
        {
            if (data == null)
                throw new ArgumentNullException("data == null");

            // 传入参数的合法性检查:可写入空间大小的检查
            if (offset + length > data.Length)
                throw new ArgumentOutOfRangeException("offset + length > data.Length");

            // expand buffer
            while (length > GetFreeCapacity())
            {
                EnsureCapacity(m_Buffer.Length + 1);
            }

            if (m_Head + length <= m_Buffer.Length)
            {
                System.Buffer.BlockCopy(data, offset, m_Buffer, m_Head, length);
            }
            else
            {
                int countToEnd = m_Buffer.Length - m_Head;
                System.Buffer.BlockCopy(data, offset, m_Buffer, m_Head, countToEnd);
                System.Buffer.BlockCopy(data, countToEnd, m_Buffer, 0, length - countToEnd);
            }
            m_Head = (m_Head + length) & m_IndexMask;

            return true;
        }

        internal bool Write(byte[] data)
        {
            return Write(data, 0, data.Length);
        }

        // 获取buff，可以写入指定大小且连续(length)的数据
        // param: [out]buf, buffer to write
        // param: [out]offset, the position where can be written
        // param: [in]length, the length of write, expand buffer's capacity internally if necessary
        // return: true if expanding capacity; return false, otherwise
        internal bool FetchBufferToWrite(out byte[] buf, out int offset, int length)
        {
            bool isExpandCapacity = false;
            while (length > GetContinuousFreeCapacityToEnd() && length > GetContinuousFreeCapacityFromStart())
            {
                EnsureCapacity(m_Buffer.Length + 1);
                isExpandCapacity = true;
            }

            int countToEnd = GetContinuousFreeCapacityToEnd();
            if(countToEnd > 0 && length > countToEnd)
            { // 尾端空间不够则插入fence
                m_Fence = m_Head;
                m_Head = 0;     // skip the remaining buffer, start from beginning
            }

            offset = m_Head;
            buf = m_Buffer;
            return isExpandCapacity;
        }

        internal void FinishWrite(int length)
        {
            m_Head = (m_Head + length) % m_IndexMask;
        }
    }
}
