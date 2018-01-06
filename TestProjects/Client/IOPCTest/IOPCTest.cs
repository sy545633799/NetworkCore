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
            GameClient client = new GameClient(65536);
            client.connectSucess += connectCompleted;
            client.connectError += connectError;
            client.ReceiveCompleted += ReceiveCompleted;
            client.Connect("192.168.5.119", 6650);

            for (int i = 0; i < 1000; i++)
            {
                client.SendAsync(BinaryUtil.IntToByte(i));
            }
            Console.ReadKey();

            client.Close();
        }

        private static void connectError(object sender, SocketAsyncEventArgs e)
        {
            Console.WriteLine("连接失败: " + e.SocketError);
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
