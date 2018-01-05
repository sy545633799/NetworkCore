using NetworkCore.IOCP;
using System;
using System.Net.Sockets;

namespace GameServer
{
    class Program
    {
        static void Main(string[] args)
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
            
        }

        private static void ReceiveCompleted(object sender, MessageEventArgs e)
        {
            
        }

        private static void AcceptCompleted(object sender, SocketAsyncEventArgs e)
        { 
            
        }
    }
}
