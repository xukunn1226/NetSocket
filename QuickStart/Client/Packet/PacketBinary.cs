using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetWorkApplication
{
    public class PacketBinary : IPacket<byte[]>
    {
        public bool Deserialize(in byte[] data, int offset, int length, out int realLength, out byte[] msg)
        {
            realLength = 0;
            msg = null;
            return false;
        }

        public byte[] Serialize(byte[] msg)
        {
            return null;
        }

        public void Serialize(byte[] msg, System.IO.MemoryStream output)
        { }

        public int CalculateSize(byte[] msg)
        {
            return 0;
        }
    }
}
