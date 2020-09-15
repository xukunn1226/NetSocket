using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.NetWork
{
    public interface IMessage
    {
        int sequenceID { get; }
    }
}
