using System.IO;
using Framework.NetWork.Log;
using Google.Protobuf;
using NetProtocol;

namespace Framework.NetWork
{
    /// <summary>
    /// Packet format: length + message
    /// </summary>
    public class PacketProtobuf : IPacket<IMessage>
    {
        public bool Deserialize(in byte[] data, int offset, int length, out int realLength, out IMessage msg)
        {
            ushort len = System.BitConverter.ToUInt16(data, offset);        // 包头数据：数据的字节流长度
            if(len <= 2)
            {
                Trace.Error("msg len <= 2");

                realLength = 0;
                msg = default(IMessage);
                return false;
            }

            if(len < length)
            { // 接收到的字节流长度小于消息字节数
                realLength = 0;
                msg = default(IMessage);
                return false;
            }

            msg = new StoreRequest();
            msg.MergeFrom(data, offset, len);            
            realLength = len;
            return true;
        }

        public byte[] Serialize(IMessage msg)
        {
            // assemble packet            
            int len = msg.CalculateSize();      // proto buf消息长度
            byte[] buf = new byte[2 + len];

            System.BitConverter.GetBytes(len).CopyTo(buf, 0);       // copy "len" to buf
            msg.ToByteArray().CopyTo(buf, 2);                       // copy "data" to buf

            return buf;
        }

        public void Serialize(IMessage msg, MemoryStream output)
        {
            ushort len = (ushort)msg.CalculateSize();
            byte[] buf = System.BitConverter.GetBytes(len);
            output.Write(buf, 0, buf.Length);
            msg.WriteTo(output);
        }

        public int CalculateSize(IMessage msg)
        {
            return 2 + msg.CalculateSize();     // 2: ushort
        }
    }
}
