using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.NetWork
{
    public class StringPacket : IPacket<string>
    {
        public bool Deserialize(in byte[] data, int offset, int length, out int realLength, out string msg)
        {
            realLength = length;
            msg = Encoding.ASCII.GetString(data, offset, length);
            return true;
        }

        public byte[] Serialize(string msg)
        {
            return Encoding.ASCII.GetBytes(msg);
        }
    }
}
