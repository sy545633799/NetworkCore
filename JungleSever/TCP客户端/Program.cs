using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace TCP客户端
{
    class Program
    {
        static void Main(string[] args)
        {
            Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            clientSocket.Connect(new IPEndPoint(IPAddress.Parse("192.168.1.5"), 88));

            byte[] data = new byte[1024];
            int count = clientSocket.Receive(data);
            string msg = Encoding.UTF8.GetString(data, 0, count);
            Console.Write(msg);

            //while (true)
            //{
            //    string s = Console.ReadLine();
            //    //Console.Write(s);
            //    if (s == "c")
            //    {
            //        clientSocket.Close();return;
            //    }
            //    clientSocket.Send(Encoding.UTF8.GetBytes(s));
            //}

            for (int i = 0; i < 100; i++)
            {
                clientSocket.Send( Message.GetBytes(i.ToString()+"长度") );
            }

            //string s = @"的数量看是的离开房间史蒂芬蓝色的咖啡机的数量看风景收代理费就是梁方就收到了负空间水电费了斯洛伐克束带结发流口水的减肥了速度快解放了速度快解放收到了客服就水电费了考试点击发山丁路口计费收到了客服技术的离开房间收到了客服就是的离开房间史蒂芬蓝色的咖啡机";

            //clientSocket.Send(Encoding.UTF8.GetBytes(s));

            Console.ReadKey();
            clientSocket.Close();
        }
    }
}
