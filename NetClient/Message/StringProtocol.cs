using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.NetWork
{
    public class StringProtocol : IProtocol<string>
    {
        public string Deserialize(ref byte[] data, int offset, int length, out int realLength)
        {
            realLength = length;
            return null;
        }

        public byte[] Serialize(string msg)
        {
            return null;
        }
    }
}
