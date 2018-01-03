//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net.WebSockets;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;
//using System.Web;
//using System.Web.WebSockets;


//namespace WebApplicationWebsocketHandler
//{
//    /// <summary>
//    /// 离线消息
//    /// </summary>
//    public class MessageInfo
//    {
//        public MessageInfo(DateTime _MsgTime, ArraySegment<byte> _MsgContent)
//        {
//            MsgTime = _MsgTime;
//            MsgContent = _MsgContent;
//        }
//        public DateTime MsgTime { get; set; }
//        public ArraySegment<byte> MsgContent { get; set; }
//    }




//    /// <summary>
//    /// Handler1 的摘要说明
//    /// </summary>
//    public class Handler1 : IHttpHandler
//    {
//        private static Dictionary<string, WebSocket> CONNECT_POOL = new Dictionary<string, WebSocket>();//用户连接池
//        private static Dictionary<string, List<MessageInfo>> MESSAGE_POOL = new Dictionary<string, List<MessageInfo>>();//离线消息池
//        public void ProcessRequest(HttpContext context)
//        {
//            //context.Response.ContentType = "text/plain";
//            //context.Response.Write("Hello World");
//            if (context.IsWebSocketRequest)
//            {
//                context.AcceptWebSocketRequest(ProcessChat);
//            }
//        }

//        private async Task ProcessChat(AspNetWebSocketContext context)
//        {
//            WebSocket socket = context.WebSocket;
//            string user = context.QueryString["user"].ToString();

//            try
//            {
//                #region 用户添加连接池
//                //第一次open时，添加到连接池中
//                if (!CONNECT_POOL.ContainsKey(user))
//                    CONNECT_POOL.Add(user, socket);//不存在，添加
//                else
//                    if (socket != CONNECT_POOL[user])//当前对象不一致，更新
//                    CONNECT_POOL[user] = socket;
//                #endregion

//                #region 离线消息处理
//                if (MESSAGE_POOL.ContainsKey(user))
//                {
//                    List<MessageInfo> msgs = MESSAGE_POOL[user];
//                    foreach (MessageInfo item in msgs)
//                    {
//                        await socket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(item.MsgTime + ":" + item.MsgContent)), WebSocketMessageType.Text, true, CancellationToken.None);
//                    }
//                    MESSAGE_POOL.Remove(user);//移除离线消息
//                }
//                #endregion

//                string descUser = string.Empty;//目的用户
//                while (true)
//                {
//                    if (socket.State == WebSocketState.Open)
//                    {
//                        ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[2048]);
//                        WebSocketReceiveResult result = await socket.ReceiveAsync(buffer, CancellationToken.None);

//                        #region 消息处理（字符截取、消息转发）
//                        try
//                        {
//                            #region 关闭Socket处理，删除连接池
//                            if (socket.State != WebSocketState.Open)//连接关闭
//                            {
//                                if (CONNECT_POOL.ContainsKey(user)) CONNECT_POOL.Remove(user);//删除连接池
//                                break;
//                            }
//                            #endregion

//                            string userMsg = Encoding.UTF8.GetString(buffer.Array, 0, result.Count);//发送过来的消息
//                            string[] msgList = userMsg.Split('|');
//                            if (msgList.Length == 2)
//                            {
//                                if (msgList[0].Trim().Length > 0)
//                                    descUser = msgList[0].Trim();//记录消息目的用户
//                                buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(msgList[1]));

//                                if (CONNECT_POOL.ContainsKey(descUser))//判断客户端是否在线
//                                {
//                                    WebSocket destSocket = CONNECT_POOL[descUser];//目的客户端
//                                    if (destSocket != null && destSocket.State == WebSocketState.Open)
//                                        await destSocket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
//                                }
//                                else
//                                {
//                                    Task.Run(() =>
//                                    {
//                                        if (!MESSAGE_POOL.ContainsKey(descUser))//将用户添加至离线消息池中
//                                            MESSAGE_POOL.Add(descUser, new List<MessageInfo>());
//                                        MESSAGE_POOL[descUser].Add(new MessageInfo(DateTime.Now, buffer));//添加离线消息
//                                    });
//                                }

//                            }
//                            else
//                            {
//                                buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(userMsg));

//                                foreach (KeyValuePair<string, WebSocket> item in CONNECT_POOL)
//                                {
//                                    await item.Value.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
//                                }

//                            }

//                        }
//                        catch (Exception exs)
//                        {
//                            //消息转发异常处理，本次消息忽略 继续监听接下来的消息
//                        }
//                        #endregion
//                    }
//                    else
//                    {
//                        break;
//                    }
//                }//while end
//            }
//            catch (Exception ex)
//            {
//                //整体异常处理
//                if (CONNECT_POOL.ContainsKey(user)) CONNECT_POOL.Remove(user);
//            }
//        }


//        public bool IsReusable
//        {
//            get
//            {
//                return false;
//            }
//        }
//    }
//}