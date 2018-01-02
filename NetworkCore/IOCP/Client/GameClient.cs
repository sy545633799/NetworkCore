using System;
using System.Net;
using System.Net.Sockets;


namespace NetworkCore.IOCP
{
    public class GameClient: BaseClient
    {
        public IPEndPoint iPEndPoint { get; set; }
        ///// <summary>
        ///// 连接完成时引发事件。
        ///// </summary>
        public event EventHandler<SocketAsyncEventArgs> connectCompleted;

        public GameClient(int size)
            : base(size)
        {
            ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public void Connect(string ip, int port)
        {
            //判断是否已连接
            if (IsConnected)
                throw new InvalidOperationException("已连接至服务器。");
            else
            {
                iPEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
                StartConnect(null);
            }
        }

        private void StartConnect(SocketAsyncEventArgs e)
        {
            if (e == null)
            {
                e = new SocketAsyncEventArgs() { RemoteEndPoint = iPEndPoint };
                e.Completed += ConnectEventArg_Completed;
                e.UserToken = this;
            }

            bool willRaiseEvent = ClientSocket.ConnectAsync(e);
            //如果没有挂起
            if (!willRaiseEvent)
                ProcessConnect(e);
        }

        private void ConnectEventArg_Completed(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                ProcessConnect(e);
            }
            catch(Exception E)
            {
                Console.WriteLine(E.Message);
            }
        }

        private void ProcessConnect(SocketAsyncEventArgs e)
        {
            connectCompleted?.Invoke(this, e);
            StartReceive(null);
        }

    }
}
