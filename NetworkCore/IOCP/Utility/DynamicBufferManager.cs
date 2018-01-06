using System;
using System.Linq;

namespace NetworkCore.IOCP
{
    public class DynamicBufferManager
    {
        private int headSize = sizeof(int);
        public byte[] readCache { get; private set; }
        public int dataLenth { get; private set; }
        private MessageEventArgs readArgs;

        //public byte[] writeCache { get; private set; }

        public DynamicBufferManager(int maxSize)
        {
            readCache = new byte[maxSize];
            readArgs = new MessageEventArgs();
        }

        public void ProcessRead(byte[] buffer, int count, EventHandler<MessageEventArgs> callback, BaseClient userToken) //接收异步事件返回的数据，用于对数据进行缓存和分包
        {
            if (dataLenth + count > readCache.Length)
            {
                Console.WriteLine("数组长度不够");
                return;
            }
            Array.Copy(buffer, 0, readCache, dataLenth, count);
            dataLenth += count;
            while (true)
            {
                if (dataLenth < headSize) break;
                int messageLenth = BitConverter.ToInt32(readCache, 0);
                if (dataLenth - headSize >= messageLenth)
                {
                    readArgs.UserToken = userToken;
                    readArgs.SetBuffer(readCache, headSize, messageLenth);
                    callback(this, readArgs);
                    Array.Copy(readCache, messageLenth + headSize, readCache, 0, dataLenth - headSize - messageLenth);
                    dataLenth -= (messageLenth + headSize);
                }
                else break;
            }
        }

        public byte[] ProcessWrite(byte[] dataBytes)
        {
            int dataLength = dataBytes.Length;
            byte[] lengthBytes = BitConverter.GetBytes(dataLength);
            byte[] newBytes = lengthBytes.Concat(dataBytes).ToArray();
            return newBytes;
        }

    }
}
