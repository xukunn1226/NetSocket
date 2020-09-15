﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.NetWork
{
    public interface IProtocol<T> where T : class
    {
        T Deserialize(in byte[] data, int offset, int length, out int realLength);
        byte[] Serialize(T msg);
    }
}