﻿using System.IO;

namespace NetWorkApplication
{
    public interface IPacket<T> where T : class
    {
        /// <summary>
        /// 字节流反序列化为消息对象
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="realLength">实际解析的长度</param>
        /// <param name="msg"></param>
        /// <returns></returns>
        bool    Deserialize(in byte[] data, int offset, int length, out int realLength, out T msg);

        /// <summary>
        /// 序列化消息对象到字节流
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        byte[]  Serialize(T msg);

        /// <summary>
        /// 序列化消息对象到MemoryStream
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="output"></param>
        void    Serialize(T msg, MemoryStream output);

        /// <summary>
        /// IPacket包的字节长度
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        int     CalculateSize(T msg);
    }
}
