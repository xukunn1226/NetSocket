using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Framework.NetWork;
using Google.Protobuf;

namespace Framework.NetWork
{
    class PacketProtobuf : IPacket<IMessage>
    {
        public bool Deserialize(in byte[] data, int offset, int length, out int realLength, out IMessage msg)
        {
            realLength = length;
            msg = null;
            return true;
        }

        public byte[] Serialize(IMessage msg)
        {
            // assemble packet
            MemoryStream ms = new MemoryStream();
            

            //return Encoding.ASCII.GetBytes(msg);
            return null;
        }
    }
}
