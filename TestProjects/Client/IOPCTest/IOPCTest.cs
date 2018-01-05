using NetworkCore.IOCP;
using NetworkCore.Utility;
using System;
using System.Net.Sockets;
using System.Text;

namespace Client.IOPCTest
{
    public class IOPCTest
    {
        public static void Client()
        {
            GameClient client = new GameClient(1024 * 1024);
            client.connectCompleted += connectCompleted;
            client.ReceiveCompleted += ReceiveCompleted;
            client.Connect("127.0.0.1", 6650);

            //Person person = new Person();
            //person.Age = 11;
            //person.Name = "李四";
            //person.Job = "总统";

            //client.Send(BinaryUtil.ObjectToBinary(person));
            for (int i = 0; i < 1000; i++)
            {
                client.Send(BinaryUtil.IntToByte(i));
            }
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
