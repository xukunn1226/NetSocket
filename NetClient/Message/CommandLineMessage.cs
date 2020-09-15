using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.NetWork
{
    public class CommandLineMessage : IMessage
    {
        public int sequenceID { get; private set; }

        public readonly string      CmdName;
        public readonly string[]    Parameters;

        public CommandLineMessage(int seqID, string cmdName, params string[] parameters)
        {
            sequenceID = seqID;
            CmdName = cmdName;
            Parameters = parameters;
        }


        public IMessage Deserialize(ref byte[] data, int offset, int length, out int realLength)
        {
            realLength = length;
            return null;
        }

        public byte[] Serialize(IMessage msg)
        {
            return new byte[1];
        }
    }
}
