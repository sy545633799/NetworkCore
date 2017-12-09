using NetworkCore.Common;
using System.Net.Sockets;
using System.Net.Security;
using System.Threading;

namespace NetworkCore.Tcp
{
    public class UserToken : ClientBase
    {
        internal UserToken(TCPListener listener, Socket socket)
            : base(socket, listener.Handler)
        {
            //创建Socket网络流
            Stream = new NetworkStream(socket);
            if (listener.IsUseAuthenticate)
            {
                NegotiateStream negotiate = new NegotiateStream(Stream);
                negotiate.AuthenticateAsServer();
                while (!negotiate.IsMutuallyAuthenticated)
                {
                    Thread.Sleep(10);
                }
            }
            //设置服务器
            Listener = listener;

            //开始异步接收数据
            SocketAsyncState state = new SocketAsyncState();
            Handler.BeginReceive(Stream, EndReceive, state);
        }

        public TCPListener Listener { get; private set; }
    }
}
