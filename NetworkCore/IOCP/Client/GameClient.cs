using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace NetworkCore.IOCP
{
    public class GameClient: BaseClient
    {
        public IPEndPoint iPEndPoint { get; set; }
        ///// <summary>
        ///// 连接完成时引发事件。
        ///// </summary>
        public event EventHandler<SocketAsyncEventArgs> connectSucess;
        /// <summary>
        /// 连接发生错误
        /// </summary>
        public event EventHandler<SocketAsyncEventArgs> connectError;

        public int TimeOut { get; set; }
        private ManualResetEvent TimeoutObject = new ManualResetEvent(false);

        public GameClient(int receiveSize, int sendTimeout = 1000, int receiveTimeout = 2000)
            : base(receiveSize)
        {
            ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ClientSocket.SendTimeout = sendTimeout;
            ClientSocket.ReceiveTimeout = receiveTimeout;
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
            SetHeartBeat();//设置心跳参数
            TimeoutObject.Reset();

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

            if (TimeoutObject.WaitOne(5000, false))//返回true为TimeoutObject.set(), 返回false为timeout
            {
                if (IsConnected)
                    connectSucess?.Invoke(this, e);
                else
                    connectError?.Invoke(this, e); 
            }
            else   
            {
                Close(null);
                e.SocketError = SocketError.TimedOut;
                connectError?.Invoke(this, e);
            }

        }

        private void ConnectEventArg_Completed(object sender, SocketAsyncEventArgs e)
        {
            ProcessConnect(e);
        }

        private void ProcessConnect(SocketAsyncEventArgs e)
        {
            try
            {
                if (e.SocketError == SocketError.Success)
                    ReceiveAsync();
                else
                    CloseClientSocket(e);
            }
            catch (Exception E)
            {
                Console.WriteLine(E.Message);
            }
            finally
            {
                TimeoutObject.Set();
            }
        }

        /// <summary>
        /// 设置心跳
        /// </summary>
        private void SetHeartBeat()
        {
            //byte[] inValue = new byte[] { 1, 0, 0, 0, 0x20, 0x4e, 0, 0, 0xd0, 0x07, 0, 0 };// 首次探测时间20 秒, 间隔侦测时间2 秒
            byte[] inValue = new byte[] { 1, 0, 0, 0, 0x88, 0x13, 0, 0, 0xd0, 0x07, 0, 0 };// 首次探测时间5 秒, 间隔侦测时间2 秒
            ClientSocket.IOControl(IOControlCode.KeepAliveValues, inValue, null);
        }
    }
}
