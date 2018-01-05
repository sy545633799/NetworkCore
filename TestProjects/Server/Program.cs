using System;
using System.Net;
using System.Text;
using System.Net.WebSockets;
using System.Threading;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Server.Test.IOPCTest.StartListener();

            //IHttpHandler
            //WebSocketServer();
        }

        private static void HttpServer()
        {
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add("http://127.0.0.1:8080/");
            listener.Start();
            HttpListenerContext context = listener.GetContext();
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;
            string responseString = string.Format("<HTML><BODY> {0}</BODY></HTML>", DateTime.Now);
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
            //对客户端输出相应信息.
            response.ContentLength64 = buffer.Length;
            System.IO.Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            //关闭输出流，释放相应资源
            output.Close();

            listener.Stop(); //关闭HttpListener
        }

        static async void WebSocketServer()
        {
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:8080/");
            listener.Start();

            while (true)
            {
                
                HttpListenerContext context = await listener.GetContextAsync();
                Console.WriteLine("connected");

                HttpListenerWebSocketContext websocketContext = await context.AcceptWebSocketAsync(null);
                ProcessClient(websocketContext.WebSocket);
            }
        }

        static async void ProcessClient(WebSocket websocket)
        {
            var data = new byte[1500];
            var buffer = new ArraySegment<byte>(data);

            while (true)
            {
                WebSocketReceiveResult result = await websocket.ReceiveAsync(buffer, CancellationToken.None);

                if (result.CloseStatus != null)
                {
                    Console.WriteLine("socket closed");
                    websocket.Abort();
                    return;
                }

                Console.WriteLine(">>> " + Encoding.UTF8.GetString(data, 0, result.Count));
                await websocket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);

            }
        }

    }
}
