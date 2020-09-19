using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.NetWork
{
    public interface IPacket<T> where T : class
    {
        bool Deserialize(in byte[] data, int offset, int length, out int realLength, out T msg);
        byte[] Serialize(T msg);
    }
}
