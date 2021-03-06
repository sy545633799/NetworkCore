﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server.TaskTest
{

    public class StateObject
    {
        public Socket workSocket = null;
        public const int BufferSize = 1024;
        public byte[] buffer = new byte[BufferSize];
        public StringBuilder sb = new StringBuilder();
    }

    class SocketExts
    {
        public static void SocketExtsTest()
        {
            //var cmd = "add 1 2 4" + Environment.NewLine;
            //Task.Run(async delegate { await SocketExts.SendAsync(cmd); });
            //Task.Run(async delegate { await SocketExts.SendAsync(cmd); });
            //Task.Run(async delegate {
            //    await SocketExts.SendAsync("ECHO this is a test"
            //     + Environment.NewLine);
            //});
            //Task.WaitAll(Task.Delay(2000));
        }
        public async static Task<string> SendAsync(string msg, string ip = "127.0.0.1", int port = 2012)
        {
            using (var client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                var remoteEP = new IPEndPoint(IPAddress.Parse(ip), port);
                await Task.Factory.FromAsync(client.BeginConnect,
                                                  client.EndConnect, remoteEP, null);

                byte[] byteData = Encoding.ASCII.GetBytes(msg);
                var result = client.BeginSend(byteData, 0, byteData.Length, 0, _ => { }, client);
                await Task.Factory.FromAsync(result, (r) => client.EndSend(r));

                StateObject state = new StateObject();
                state.workSocket = client;
                var received = client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, _ => { }, state);
                var response = await Task<string>.Factory.FromAsync(received, ar =>
                {
                    ReceiveCallback(ar);
                    return ((StateObject)ar.AsyncState).sb.ToString();
                });


                Console.WriteLine("=======Response received : {0}=====", response);
                client.Shutdown(SocketShutdown.Both);
                client.Close();
                return response;
            }
        }
        private static void ReceiveCallback(IAsyncResult ar)
        {
            StateObject state = (StateObject)ar.AsyncState;
            Socket client = state.workSocket;
            int bytesRead = client.EndReceive(ar);
            if (bytesRead > 0)
            {
                state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));
                client.BeginReceive(state.buffer, 0, bytesRead, SocketFlags.None, new AsyncCallback(ReceiveCallback), client);
            }
        }
    }
}
