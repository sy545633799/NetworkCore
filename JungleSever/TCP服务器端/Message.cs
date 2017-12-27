using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCP服务器端
{
    class Message
    {
        private byte[] data = new byte[1024];
        private int dataLength = 0;//我们存取了多少个字节的数据在数组里面

        public void AddCount(int count)
        {
            dataLength += count;
        }
        public byte[] Data
        {
            get { return data; }
        }
        public int StartIndex
        {
            get { return dataLength; }
        }
        public int RemainSize
        {
            get { return data.Length - dataLength; }
        }
        /// <summary>
        /// 解析数据或者叫做读取数据
        /// </summary>
        public void ReadMessage()
        {
            while (true)
            {
                if (dataLength <= 4) return;
                int messageByteCount = BitConverter.ToInt32(data, 0);
                if ((dataLength - 4) >= messageByteCount)
                {
                    Console.WriteLine(dataLength);
                    Console.WriteLine(messageByteCount);
                    string s = Encoding.UTF8.GetString(data, 4, messageByteCount);
                    Console.WriteLine("解析出来一条数据：" + s);
                    Array.Copy(data, messageByteCount + 4, data, 0, dataLength - 4 - messageByteCount);
                    dataLength -= (messageByteCount + 4);
                }
                else
                {
                    break;
                }
            }
        }
    }
}
