using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.NetWork
{
    public class StringProtocol : IProtocol<string>
    {
        public string Deserialize(in byte[] data, int offset, int length, out int realLength)
        {
            realLength = length;
            return Encoding.ASCII.GetString(data, offset, length);
        }

        public byte[] Serialize(string msg)
        {
            return Encoding.ASCII.GetBytes(msg);
        }
    }
}
