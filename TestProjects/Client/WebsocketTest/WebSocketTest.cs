using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client.Test
{
    public class WebSocketTest
    {
        public static void Test()
        {
            ClientWebSocket client = new ClientWebSocket();
            client.ConnectAsync(new Uri("ws://localhost:4567/ws/"), CancellationToken.None).Wait();
            StartReceiving(client);

            string line;
            while ((line = Console.ReadLine()) != "exit")
            {
                var array = new ArraySegment<byte>(Encoding.UTF8.GetBytes(line));
                client.SendAsync(array, WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }

        static async void StartReceiving(ClientWebSocket client)
        {
            while (true)
            {
                var array = new byte[4096];
                var result = await client.ReceiveAsync(new ArraySegment<byte>(array), CancellationToken.None);
                
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    string msg = Encoding.UTF8.GetString(array, 0, result.Count);
                    Console.WriteLine("--> {0}", msg);
                }
            }
        }

        private static async Task Echo()
        {
            using (ClientWebSocket ws = new ClientWebSocket())
            {
                Uri serverUri = new Uri("ws://localhost:49889/");
                await ws.ConnectAsync(serverUri, CancellationToken.None);
                while (ws.State == WebSocketState.Open)
                {
                    Console.Write("Input message ('exit' to exit): ");
                    string msg = Console.ReadLine();
                    if (msg == "exit")
                    {
                        break;
                    }
                    ArraySegment<byte> bytesToSend = new ArraySegment<byte>(Encoding.UTF8.GetBytes(msg));
                    await ws.SendAsync(bytesToSend, WebSocketMessageType.Text, true, CancellationToken.None);
                    ArraySegment<byte> bytesReceived = new ArraySegment<byte>(new byte[1024]);
                    WebSocketReceiveResult result = await ws.ReceiveAsync(bytesReceived, CancellationToken.None);
                    Console.WriteLine(Encoding.UTF8.GetString(bytesReceived.Array, 0, result.Count));
                }
            }
        }
    }
}
