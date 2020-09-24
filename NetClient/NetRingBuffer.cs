using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.NetWork
{
    /// <summary>
    /// 非通用ringbuffer，仅适配网络传输用
    /// 在多线程中负责数据传递，因需要保持数据稳定性，故不支持动态扩容
    /// </summary>
    internal class NetRingBuffer
    {
        private const int       m_MinCapacity   = 1024;
        private byte[]          m_Buffer;
        private int             m_IndexMask;
        internal byte[]         Buffer          { get { return m_Buffer; } }
        internal int            Head            { get; set; }
        internal int            Tail            { get; set; }
        internal int            Fence           { get; set; }

        internal NetRingBuffer(int capacity = 8 * 1024)
        {
            Init(capacity);
        }

        private void Init(int min)
        {
            if (m_Buffer != null && m_Buffer.Length > 0)
                throw new Exception("NetRingBuffer has already init");

            int newCapacity = Math.Min(Math.Max(m_MinCapacity, min), Int32.MaxValue);
            newCapacity = NextPowerOfTwo(newCapacity);

            m_Buffer = new byte[newCapacity];
            m_IndexMask = m_Buffer.Length - 1;
        }

        internal void Clear()
        {
            Head = 0;
            Tail = 0;
            Fence = 0;
        }

        internal bool IsEmpty()
        {
            return Head == Tail;
        }

        internal bool IsFull()
        {
            return ((Head + 1) & m_IndexMask) == Tail;
        }

        internal int GetMaxCapacity()
        {
            return m_Buffer.Length - 1;
        }

        internal int GetFreeCapacity()
        {
            return GetMaxCapacity() - GetUsedCapacity();
        }

        internal int GetUsedCapacity()
        {
            return Head >= Tail ? Head - Tail : m_Buffer.Length - (Tail - Head);
        }

        internal int GetUsedCapacity(int head)
        {
            return head >= Tail ? head - Tail : m_Buffer.Length - (Tail - head);
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

        /// <summary>
        /// expand buffer, keep the m_Head, m_Tail, m_Fence unchanged
        /// </summary>
        /// <param name="min"></param>
        //private void EnsureCapacity(int min)
        //{
        //    if (m_Buffer == null || m_Buffer.Length < min)
        //    {
        //        int newCapacity = m_Buffer == null || m_Buffer.Length == 0 ? m_MinCapacity : m_Buffer.Length * 2;
        //        if ((uint)newCapacity > Int32.MaxValue)
        //            newCapacity = Int32.MaxValue;
        //        if (newCapacity < min)
        //            newCapacity = min;
        //        newCapacity = NextPowerOfTwo(newCapacity);

        //        // expand buffer
        //        byte[] newBuf = new byte[newCapacity];
        //        if (Head > Tail)
        //        {
        //            System.Buffer.BlockCopy(m_Buffer, Tail, newBuf, Tail, Head - Tail);
        //            //m_Tail = m_Tail;      // no change
        //            //m_Head = m_Head;      // no change
        //        }
        //        else if (Head < Tail)
        //        {
        //            int countToEnd = m_Buffer.Length - Tail;
        //            System.Buffer.BlockCopy(m_Buffer, Tail, newBuf, newBuf.Length - countToEnd, countToEnd);

        //            if (Head > 0)
        //                System.Buffer.BlockCopy(m_Buffer, 0, newBuf, 0, Head);

        //            Tail = newBuf.Length - countToEnd;
        //            //m_Head = m_Head;      // no change
        //            if (Fence > 0)
        //            {
        //                if (Fence < Tail)
        //                    throw new ArgumentException($"m_Fence{Fence} < m_Tail{Tail}");
        //                Fence = newBuf.Length - (m_Buffer.Length - Fence);
        //            }
        //        }
        //        m_Buffer = newBuf;
        //        m_IndexMask = m_Buffer.Length - 1;
        //    }
        //}

        // get continous free capacity from head to buffer end
        private int GetContinuousFreeCapacityToEnd()
        {
            return Math.Min(GetFreeCapacity(), Head >= Tail ? m_Buffer.Length - Head : 0);
        }

        // get continous free capacity from buffer start to tail
        private int GetContinuousFreeCapacityFromStart()
        {
            int count = 0;
            if (Head < Tail)
            {
                count = Tail - Head;
            }
            else if (Head > Tail)
            {
                count = Tail;
            }
            else
            {
                count = m_Buffer.Length - Head;
            }

            return Math.Min(GetFreeCapacity(), count);
        }

        private int GetContinuousUsedCapacity()
        {
            return Head >= Tail ? Head - Tail : m_Buffer.Length - Tail;
        }

        // 获取已接收到的网络数据
        internal ref readonly byte[] FetchBufferToRead(out int offset, out int length)
        {
            offset = Tail;
            length = GetContinuousUsedCapacity();
            return ref m_Buffer;
        }

        internal void FinishRead(int length)
        {
            Tail = (Tail + length) & m_IndexMask;
        }

        /// <summary>
        /// 写入待发送的数据，主线程调用
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        internal void Write(byte[] data, int offset, int length)
        {
            if (data == null)
                throw new ArgumentNullException("data == null");

            // 传入参数的合法性检查:可写入空间大小的检查
            if (offset + length > data.Length)
                throw new ArgumentOutOfRangeException("offset + length > data.Length");

            if (length > GetFreeCapacity())
                throw new System.ArgumentOutOfRangeException("NetRingBuffer is FULL, can't write anymore");
            
            if (Head + length <= m_Buffer.Length)
            {
                System.Buffer.BlockCopy(data, offset, m_Buffer, Head, length);
            }
            else
            {
                int countToEnd = m_Buffer.Length - Head;
                System.Buffer.BlockCopy(data, offset, m_Buffer, Head, countToEnd);
                System.Buffer.BlockCopy(data, countToEnd, m_Buffer, 0, length - countToEnd);
            }
            Head = (Head + length) & m_IndexMask;
        }

        /// <summary>
        /// 同上
        /// </summary>
        /// <param name="data"></param>
        internal void Write(byte[] data)
        {
            Write(data, 0, data.Length);
        }

        /// <summary>
        /// 获取连续地、制定大小(length)的buff，返回给上层写入数据，主线程调用
        /// </summary>
        /// <param name="length">the length of write, expand buffer's capacity internally if necessary</param>
        /// <param name="buf">buffer to write</param>
        /// <param name="offset">the position where can be written</param>
        internal void FetchBufferToWrite(int length, out byte[] buf, out int offset)
        {
            if (length > GetContinuousFreeCapacityToEnd() && length > GetContinuousFreeCapacityFromStart())
                throw new ArgumentOutOfRangeException($"NetRingBuffer: no space to receive data {length}");

            int countToEnd = GetContinuousFreeCapacityToEnd();
            if(countToEnd > 0 && length > countToEnd)
            { // 需要连续空间，尾端空间不够则插入fence，从0开始
                Fence = Head;
                Head = 0;     // skip the remaining buffer, start from beginning
            }

            offset = Head;
            buf = m_Buffer;
        }

        /// <summary>
        /// 数据写入缓存完成，与FetchBufferToWrite对应，同一帧调用
        /// </summary>
        /// <param name="length"></param>
        internal void FinishBufferWriting(int length)
        {
            Head = (Head + length) % m_IndexMask;
        }

        /// <summary>
        /// 撤销fence，主线程调用
        /// </summary>
        internal void ResetFence()
        {
            Fence = 0;
        }

        /// <summary>
        /// 发送数据完成，子线程调用
        /// </summary>
        /// <param name="length"></param>
        internal void FinishBufferSending(int length)
        {
            // 数据包发送完成更新m_Tail
            Tail = (Tail + length) % m_IndexMask;
        }
    }
}
