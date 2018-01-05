using NetworkCore.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkCore.Websocket.Server
{
    public class WebsocketListener
    {
        private int m_numConnections; //最大支持连接个数
        private Semaphore m_maxNumberAcceptedClients; //限制访问接收连接的线程数，用来控制最大并发数
        //private ClientPool<UserToken> m_UserTokenPool;

        //public WebsocketListener(int numConnections, int asyncBufferSize)
        //{
        //    m_numConnections = numConnections;

        //    m_UserTokenPool = new ClientPool<UserToken>(numConnections);
        //    m_maxNumberAcceptedClients = new Semaphore(numConnections, numConnections);

        //    for (int i = 0; i < m_numConnections; i++) //按照连接数建立读写对象
        //    {
        //        UserToken userToken = new UserToken(asyncBufferSize);
        //        m_UserTokenPool.Push(userToken);
        //    }

        //}

        public async void Start(string adress)
        {
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add(adress);
            listener.Start();

            while (true)
            {
                HttpListenerContext context = await listener.GetContextAsync();
                HttpListenerWebSocketContext websocketContext = await context.AcceptWebSocketAsync(null);
                //ProcessClient(websocketContext.WebSocket);
            }
        }
    }
}
