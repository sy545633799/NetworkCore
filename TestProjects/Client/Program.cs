
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            clientSocket.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 6650));

            //byte[] data = new byte[1024];
            //int count = clientSocket.Receive(data);
            //string msg = Encoding.UTF8.GetString(data, 0, count);
            //Console.Write(msg);
            //while (true)
            //{
            //    string str = Console.ReadLine();
            //    clientSocket.Send(System.Text.Encoding.UTF8.GetBytes(str));
            //}

            for (int i = 0; i < 100; i++)
            {
                clientSocket.Send(GetBytes(i.ToString()));
            }
            Console.ReadKey();
        }

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
