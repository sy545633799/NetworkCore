using System;
using System.Net.Sockets;

namespace NetworkCore.IOCP
{
    public class BaseClient
    {
        protected DynamicBufferManager Handler;
        public DateTime ConnectDateTime { get; protected set; }
        public DateTime ActiveDateTime { get; protected set; }
        public SocketAsyncEventArgs SendEventArgs { protected set; get; }
        public Socket ClientSocket { get; set; }
        /// <summary>
        /// 获取是否已连接。
        /// </summary>
        public bool IsConnected { get { return ClientSocket != null && ClientSocket.Connected; } }

        private int bufferSize;

        #region 事件

        ///// <summary>
        ///// 断开完成时引发事件。
        ///// </summary>
        public event EventHandler<SocketAsyncEventArgs> DisconnectCompleted;
        ///// <summary>
        ///// 接收完成时引发事件。
        ///// </summary>
        public event EventHandler<MessageEventArgs> ReceiveCompleted;
        ///// <summary>
        ///// 发送完成时引发事件。
        ///// </summary>
        public event EventHandler<MessageEventArgs> SendCompleted;
        /// <summary>
        /// 发生错误时引发的事件
        /// </summary>
        public event EventHandler<Exception> OnError;

        #endregion

        public BaseClient(int asyncBufferSize)
        {
            bufferSize = asyncBufferSize;
            Handler = new DynamicBufferManager();
        }

        private void IO_Completed(object sender, SocketAsyncEventArgs asyncEventArgs)
        {
            try
            {
                lock (this)
                {
                    if (asyncEventArgs.LastOperation == SocketAsyncOperation.Receive)
                        ProcessReceive(asyncEventArgs);
                    else if (asyncEventArgs.LastOperation == SocketAsyncOperation.Send)
                        ProcessSend(asyncEventArgs);
                    else
                        throw new ArgumentException("The last operation completed on the socket was not a receive or send");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public void StartReceive(SocketAsyncEventArgs receiveEventArgs)
        {
            if (receiveEventArgs == null)
            {
                receiveEventArgs = new SocketAsyncEventArgs();
                receiveEventArgs.Completed += IO_Completed;
                receiveEventArgs.UserToken = this;
                receiveEventArgs.SetBuffer(new byte[bufferSize], 0, bufferSize);
            }
            try
            {
                //用户连接对象 开启异步数据接收
                bool result = ClientSocket.ReceiveAsync(receiveEventArgs);
                //异步事件是否挂起
                if (!result)
                {
                    lock (ClientSocket)
                    {
                        ProcessReceive(receiveEventArgs);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private void ProcessReceive(SocketAsyncEventArgs receiveEventArgs)
        {
            if (ClientSocket == null)
                return;
            ActiveDateTime = DateTime.Now;
            if (receiveEventArgs.SocketError == SocketError.Success)
            {
                if (receiveEventArgs.BytesTransferred > 0)
                {
                    Handler.ProcessRead(receiveEventArgs.Buffer, receiveEventArgs.BytesTransferred, ReceiveCompleted, this);
                    StartReceive(receiveEventArgs);
                }
                else
                {
                    Console.WriteLine("客户端主动断开连接");
                    CloseClientSocket(receiveEventArgs);
                }
            }
            else
            {
                Console.WriteLine("客户端异常断开");
                CloseClientSocket(receiveEventArgs);
            }
        }

        public void Send(byte[] buffer)
        {
            if (ClientSocket == null) return;
            byte[] message = Handler.ProcessWrite(buffer);
            ClientSocket.Send(message);
        }

        #region 异步发送TODO
        private void StartSend(SocketAsyncEventArgs e)
        {
            if (e == null)
            {
                e = new SocketAsyncEventArgs();
                e.Completed += IO_Completed;
                e.UserToken = this;
            }
            e.SetBuffer(new byte[bufferSize], 0, bufferSize);
        }

        private void ProcessSend(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                
            }
            else
            {
                CloseClientSocket(e);
            }
        }
        #endregion

        public void Close()
        {
            CloseClientSocket(new SocketAsyncEventArgs() { SocketError = SocketError.Success });
        }

        protected void CloseClientSocket(SocketAsyncEventArgs e)
        {
            if (ClientSocket == null)
                return;
            try
            {
                if (IsConnected)
                {
                    ClientSocket.Shutdown(SocketShutdown.Both);
                    DisconnectCompleted?.Invoke(this, e);
                    ClientSocket.Close();
                    if (ClientSocket != null) ClientSocket.Dispose();
                    ClientSocket = null; //释放引用，并清理缓存，包括释放协议对象等资源
                }
            }
            catch (Exception E)
            {
                Console.WriteLine(E.Message);
            }
        }
    }
}
