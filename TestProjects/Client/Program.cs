using System;
using System.Net.Sockets;
using System.Text;
using NetworkCore.IOCP;
using NetworkCore.Utility;
using System.Net.WebSockets;
using System.Threading;
using System.Web;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            //string url = "ws://localhost:24900/" + "test.ashx";

            //try
            //{
            //    //System.Net.WebSockets.ClientWebSocket cln = new System.Net.WebSockets.ClientWebSocket();
            //    //cln.ConnectAsync(new Uri(url), new CancellationToken()).Wait();

            //    //cln.SendAsync(new ArraySegment<byte>("my message".GetBytesUtf8()), System.Net.WebSockets.WebSocketMessageType.Text, true, new CancellationToken()).Wait();
            //    //var ws = new ClientWebSocket();
            //    //await ws.ConnectAsync(new Uri("ws://127.0.0.1:8080"), CancellationToken.None);

            //}
            //catch (Exception ex)
            //{
            //    string ss = ex.ToString();
            //}
        }

        private static void Client()
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
