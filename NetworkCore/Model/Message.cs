using NetworkCore.Utility;

namespace NetworkCore.Model
{
    public class Message
    {
        public int ID { get; set; }

        public string CMD { get; set; }

        public byte[] Data { get; set; }

        public T Parse<T>() where T : class
        {
            if (Data == null) return default(T);
            else return BinaryUtil.ByteToObject<T>(Data);
        }
    }
}
