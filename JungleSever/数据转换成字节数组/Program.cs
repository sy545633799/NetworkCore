using System.Text;
using System;

namespace 数据转换成字节数组
{
    class Program
    {
        static void Main(string[] args)
        {
            //处理string char
            //byte[] data = Encoding.UTF8.GetBytes("10000");

            //处理值了类型
            int count = 1000000 ;
            byte[] data = BitConverter.GetBytes(count);
            
            foreach (byte b in data)
            {
                Console.Write(b + ":");
            }
            Console.ReadKey();
        }
    }
}
