using NetworkCore.Wodsoft.Common;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace NetworkCore.Wodsoft.Tcp
{
    /// <summary>
    /// TCP监听端
    /// </summary>
    public class TCPListener : IEnumerable<UserToken>, IDisposable
    {
        private Socket socket;
        private HashSet<UserToken> clients;

        /// <summary>
        /// 实例化TCP监听者。
        /// </summary>
        public TCPListener(ISocketHandler handler)
        {
            clients = new HashSet<UserToken>();
            IsStarted = false;
            Handler = handler;
            IsUseAuthenticate = false;
        }

        public ISocketHandler Handler { get; set; }

        public bool IsUseAuthenticate { get; set; }

        public int Count { get { return clients.Count; } }

        private int _port;

        /// <summary>
        /// 服务启动中
        /// </summary>
        public bool IsStarted { get; private set; }

        /// <summary>
        /// 开始服务。
        /// </summary>
        public void Start(int port)
        {
            lock (this)
            {
                if (IsStarted)
                    throw new InvalidOperationException("已经开始服务。");
                _port = port;
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //绑定端口
                //可以引发端口被占用异常
                socket.Bind(new IPEndPoint(IPAddress.Any, _port));
                //监听队列
                socket.Listen(ushort.MaxValue);
                //如果端口是0，则是随机端口，把这个端口赋值给port
                _port = ((IPEndPoint)socket.LocalEndPoint).Port;
                //服务启动中设置为true
                IsStarted = true;
                //开始异步监听
                socket.BeginAccept(EndAccept, null);
            }
        }

        //异步监听结束
        private void EndAccept(IAsyncResult result)
        {
            
            Socket userToken = null;

            //获得客户端Socket
            try
            {
                userToken = socket.EndAccept(result);
                socket.BeginAccept(EndAccept, null);
            }
            catch
            {

            }

            if (userToken == null)
                return;

            //实例化客户端类
            UserToken client = new UserToken(this, userToken);
            //增加事件钩子
            client.SendCompleted += client_SendCompleted;
            client.ReceiveCompleted += client_ReceiveCompleted;
            client.DisconnectCompleted += client_DisconnectCompleted;

            //增加客户端
            lock (clients)
                clients.Add(client);

            //客户端连接事件
            AcceptCompleted?.Invoke(this, new SocketEventArgs(client, SocketAsyncOperation.Accept));
        }

        /// <summary>
        /// 停止服务。
        /// </summary>
        public void Stop()
        {
            lock (this)
            {
                if (!IsStarted)
                    throw new InvalidOperationException("没有开始服务。");
                foreach (UserToken client in clients)
                {
                    client.Disconnect();
                    client.DisconnectCompleted -= client_DisconnectCompleted;
                    client.ReceiveCompleted -= client_ReceiveCompleted;
                    client.SendCompleted -= client_SendCompleted;
                }
                socket.Close();
                socket = null;
                IsStarted = false;
            }
        }

        /// <summary>
        /// 接收完成时引发事件。
        /// </summary>
        public event EventHandler<SocketEventArgs> ReceiveCompleted;
        /// <summary>
        /// 接受客户完成时引发事件。
        /// </summary>
        public event EventHandler<SocketEventArgs> AcceptCompleted;
        /// <summary>
        /// 客户断开完成时引发事件。
        /// </summary>
        public event EventHandler<SocketEventArgs> DisconnectCompleted;
        /// <summary>
        /// 发送完成时引发事件。
        /// </summary>
        public event EventHandler<SocketEventArgs> SendCompleted;

        //客户端断开连接
        private void client_DisconnectCompleted(object sender, SocketEventArgs e)
        {
            //移除客户端
            lock (clients)
                clients.Remove((UserToken)e.Client);

            e.Client.DisconnectCompleted -= client_DisconnectCompleted;
            e.Client.ReceiveCompleted -= client_ReceiveCompleted;
            e.Client.SendCompleted -= client_SendCompleted;
            DisconnectCompleted?.Invoke(this, e);
        }

        //收到客户端发送的数据
        private void client_ReceiveCompleted(object sender, SocketEventArgs e)
        {
            ReceiveCompleted?.Invoke(this, e);
        }

        //向客户端发送数据完成
        private void client_SendCompleted(object sender, SocketEventArgs e)
        {
            SendCompleted?.Invoke(this, e);
        }

        /// <summary>
        /// 获取客户端泛型。
        /// </summary>
        /// <returns></returns>
        public IEnumerator<UserToken> GetEnumerator()
        {
            return clients.GetEnumerator();
        }

        /// <summary>
        /// 获取客户端泛型。
        /// </summary>
        /// <returns></returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return clients.GetEnumerator();
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (socket == null)
                return;
            Stop();
        }
    }
}
