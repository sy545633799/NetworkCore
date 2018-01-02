using System;
using System.Net.Sockets;
using System.Text;
using NetworkCore.IOCP;
using NetworkCore.Utility;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            GameClient client = new GameClient(1024 * 1024);
            client.connectCompleted += connectCompleted;
            client.ReceiveCompleted += ReceiveCompleted;
            client.Connect("127.0.0.1", 6650);

            Person person = new Person();
            person.Age = 11;
            person.Name = "李四";
            person.Job = "总统";

            client.Send(BinaryUtil.ObjectToBinary(person));
            Console.ReadKey();

            client.Close();
        }

        private static void ReceiveCompleted(object sender, MessageEventArgs e)
        {
            Console.WriteLine("ClientReceive:" + Encoding.UTF8.GetString(e.Data, 0, e.DataLenth));
        }

        private static void connectCompleted(object sender, SocketAsyncEventArgs e)
        {
            Console.WriteLine("连接成功");
        }
    }
}
