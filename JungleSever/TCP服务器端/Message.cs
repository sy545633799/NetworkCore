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
        private int dataLenth = 0;//我们存取了多少个字节的数据在数组里面

        public byte[] Data
        {
            get { return data; }
        }
        public int StartIndex
        {
            get { return dataLenth; }
        }
        public int RemainSize
        {
            get { return data.Length - dataLenth; }
        }
        /// <summary>
        /// 解析数据或者叫做读取数据
        /// </summary>
        public void ReadMessage(int count)
        {
            dataLenth +=count;
            while (true)
            {
                if (dataLenth <= 4) return;
                int messageByteCount = BitConverter.ToInt32(data, 0);
                if ((dataLenth - 4) >= messageByteCount)
                {
                    Console.WriteLine(dataLenth);
                    Console.WriteLine(messageByteCount);
                    string s = Encoding.UTF8.GetString(data, 4, messageByteCount);
                    Console.WriteLine("解析出来一条数据：" + s);
                    Array.Copy(data, messageByteCount + 4, data, 0, dataLenth - 4 - messageByteCount);
                    dataLenth -= (messageByteCount + 4);
                }
                else
                {
                    break;
                }
            }
        }
    }
}
