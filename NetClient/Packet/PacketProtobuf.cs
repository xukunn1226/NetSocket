using System.IO;
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

        public void Serialize(IMessage msg, MemoryStream output)
        {

        }

        public int CalculateSize(IMessage msg)
        {
            return msg.CalculateSize();
        }
    }
}
