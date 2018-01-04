using NetworkCore.IOCP;
using System;
using System.Net.Sockets;
using System.Text;
using NetworkCore.Utility;

namespace Server.Test
{
    public class IOPCTest
    {
        public static void StartListener()
        {
            GameListener listener = new GameListener(10, 1024 * 1024);
            listener.AcceptCompleted += AcceptCompleted;
            listener.ReceiveCompleted += ReceiveCompleted;
            listener.DisconnectCompleted += DisconnectCompleted;
            listener.Start(6650);

            Console.ReadKey();
        }

        private static void DisconnectCompleted(object sender, SocketAsyncEventArgs e)
        {
            //Console.WriteLine("客户端断开：" + e.SocketError);
        }

        private static void ReceiveCompleted(object sender, MessageEventArgs message)
        {
            Console.WriteLine("receive:" + BinaryUtil.ByteToInt(message.Data).ToString());
        }

        private static void AcceptCompleted(object sender, SocketAsyncEventArgs e)
        {
            Console.WriteLine("客户端连接");
            UserToken userToken = e.UserToken as UserToken;
        }
    }
}
