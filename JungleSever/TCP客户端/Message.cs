using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCP客户端
{
    class Message
    {
        public static byte[] GetBytes(string data)
        {
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);
            int dataLength = dataBytes.Length;
            byte[] lengthBytes = BitConverter.GetBytes(dataLength);
            byte[] newBytes = lengthBytes.Concat(dataBytes).ToArray();
            return newBytes;
        }
    }
}
