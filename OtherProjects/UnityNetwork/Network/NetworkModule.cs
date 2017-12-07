using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using UnityEngine;

public enum GameZone
{
    Release = 1,
    Develop = 2,
}

public enum GameServer
{
    [Description("香格里拉开发服")]
    Develop = 1,
    [Description("机房体验服")]
    Experience = 2,
    [Description("机房预发布服")]
    PreRelease = 3,
    [Description("机房正式服")]
    Release = 4,
    [Description("机房审核服")]
    Scrutinize = 5,
}

public class NetworkModule : MonoBehaviour
{
    private TcpClient mClient = null;
    public bool Connected { get { return mClient != null && mClient.Connected; } }
    public bool Connecting { get { return mClient != null && mClient.Connecting; } }

    private bool mInLoginStage = true;
    private string mHost;
    private Int32 mPort;
    private string mAccount;
    private UInt32 mAccid;
    private string mPassToken;
    private string mSessToken;
    private Int64 mFreezeTime;

    public string Account { get { return mAccount; } set { mAccount = value; } }
    public UInt32 Accid { get { return mAccid; } }
    public string PassToken { get { return mPassToken; } set { mPassToken = value; } }
    public string SessToken { get { return mSessToken; } }
    public Int64 FreezeTime { get { return mFreezeTime; } }

    void Start()
    {
        
    }

    void OnDestroy()
    {
        CancelInvoke("SyncTimeStamp");
        Close();
    }

    void Update()
    {
        //while (mClient != null && !LesLoadingHelper.IsLoading && mClient.ProcessPacket()) ;
        while (mClient != null && mClient.ProcessPacket()) ;
    }

    public void Connect(string host, int port, Action<string> handler)
    {
        if (mClient != null)
            Close();

        mHost = host;
        mPort = port;
        mClient = new TcpClient();
        mClient.connect(host, port, handler);
        mClient.OnCloseHandler = OnDisconnected;
    }

    public void Close()
    {
        if (mClient != null)
            mClient.close();
    }

    void SyncTimeStamp()
    {
        //Request<RequestSyncTimeStampCmd>(new RequestSyncTimeStampCmd(), (err, resp) =>
        //{
        //    Invoke("SyncTimeStamp", 60);
        //    string error = LesUIHelper.Translate(err);
        //    if (!string.IsNullOrEmpty(error))
        //    {
        //        LesUIHelper.SystemMessage(error);
        //        Debug.LogError("SyncTimeStamp: " + error);
        //        return;
        //    }

        //    LesTimeHelper.UpdateServerTime(resp.server, LesTimeHelper.LocalMSecond() - resp.client);
        //});

        Request(new RequestSyncTimeStampCmd(), null);
    }

    // 创建账户
    public void Create(string account, string token, Action<string> handler)
    {
        Request(new RequestCreateAccountCmd(account, MD5Crypto.MD5Encoding(account + token)), (string err, Response resp) => { handler(err); });
    }

    // 自动创建游客账户
    public void CreateGuest(Action<string, CreateGuestAccountReply> handler)
    {
        Request(new RequestCreateGuestAccountCmd(), handler);
    }

    public void BindGuest(string guestAccount, string guestToken, string newAccount, string newPassToken, Action<string> handler)
    {
        newPassToken = MD5Crypto.MD5Encoding(newAccount + newPassToken);

        // 非游客账户不允许绑定
        if (!guestAccount.ToLower().Contains(Constant.GUEST_PREFIX))
        {
            handler("GuestOnly");
            return;
        }

        // 玩家注册的账户名不能包含游客账户信息
        if (newAccount.ToLower().Contains(Constant.GUEST_PREFIX))
        {
            handler("IllegalityAccount");
            return;
        }

        Request(new RequestBindGuestAccountCmd(guestAccount, guestToken, newAccount, newPassToken), (err, resp) => 
        {
            if (string.IsNullOrEmpty(err))
            {
                mAccount = newAccount;
                mPassToken = newPassToken;
            }
            handler(err); 
        });
    }

    // 登录账户
    public void Login(string account, string token, string sdk, Action<string, LesPlayerData> handler)
    {
        mInLoginStage = true;

        Request<LoginReply>(new RequestLoginCmd(account, token, sdk), (string err, LoginReply reply) =>
        {
            string error = LesUIHelper.Translate(err);
            if (!string.IsNullOrEmpty(error))
            {
                handler(err, null);
                LesUIHelper.SystemMessage("登陆失败");
                return;
            }

            mAccount = account;
            mAccid = reply.accid;
            mPassToken = token;
            mSessToken = reply.token;
            Debug.Log("LoginReply: " + mAccid + " " + mSessToken);

            LesTimeHelper.SetServerTime(reply.server_time, reply.server_zone);

            Select((int)LesGameCore.Instance.Zone, handler);
        });
    }

    // 重连游戏
    void Relogin(Action<string, LesPlayerData> handler)
    {
        if (mInLoginStage)
        {
            Debug.LogWarning("InLoginStage...");
            handler("NeedLogin", null);
            return;
        }

        Debug.Log("relogin use: " + mHost + " " + mPort + " " + mSessToken);

        Connect(mHost, mPort, (err) =>
        {
            if (!string.IsNullOrEmpty(err))
            {
                handler(err, null);
                return;
            }

            // 注册消息推送回调
            MessageHandler.Register();

            // 请求重连
            Request<LesPlayerData>(new RequestReloginCmd(mAccid, mSessToken, LesPlayerDataManager.Instance.Scene), (e, player_data) =>
            {
                handler(e, player_data);
            });
        });
    }

    // 选择大区服
    void Select(int zone, Action<string, LesPlayerData> handler)
    {
        mFreezeTime = 0;

        Request<SelectZoneReply>(new RequestSelectZoneCmd(zone), (string err, SelectZoneReply reply) =>
        {
            // 选择大区，服务器返回对应大区的连接信息
            if (!string.IsNullOrEmpty(err))
            {
                // 大区不可用
                handler(err, null);
                return;
            }

            Debug.Log("baseapp: " + reply.host + ":" + reply.port);

            // 使用返回的连接信息去连接服务器
            Connect(reply.host, reply.port, (string e) =>
            {
                if (!string.IsNullOrEmpty(e))
                {
                    // 连接网关服务失败！
                    handler(e, null);
                    return;
                }

                // 注册消息推送回调
                MessageHandler.Register();

                // 请求登录网关服务器
                Request(new RequestLoginBaseAppCmd(mAccid, mSessToken), (string le, Response resp) =>
                {
                    if (!string.IsNullOrEmpty(le))
                    {
                        // 限时冻结 = 7067
                        if ("7067" == le)
                        {
                            var freezeInfo = resp.Parse<FreezeInfo>();
                            mFreezeTime = freezeInfo.time;
                        }

                        // 登录网关服务失败！
                        handler(le, null);
                        return;
                    }

                    // TODO: 如果没有选角色的流程那么这里就不用这一步了
                    // 登录网关服务器成功，请求角色列表
                    mInLoginStage = false;
                    InvokeRepeating("SyncTimeStamp", 1, 20);

                    // 直接获得角色数据
                    handler(le, resp.Parse<LesPlayerData>());
                });
            });
        });
    }

    // 注册指定服务器推送消息
    public void Register<T>(Action<T> action) where T : ICommand
    {
        mClient.Register<T>(action);
    }

    // 注册指定服务器推送消息
    public void Register(ERequestTypes eventId, Action<ICommand> action)
    {
        mClient.Register(eventId, action);
    }

    public void Request(RequestCmd cmd, Action<string, Response> handler)
    {
        mClient.request(cmd, handler);
    }

    public void Request<T>(RequestCmd cmd, Action<string, T> handler) where T : ICommand
    {
        mClient.request(cmd, (string err, Response resp) =>
        {
            ICommand response = resp;
            if (resp != null && typeof(T) != typeof(Response))
                response = resp.Parse<T>();
            handler(err, (T)response);
        });
    }

    private void OnDisconnected(string reason)
    {
        if (mInLoginStage)
        {
            Debug.Log("LoginStage: " + reason);
            LesUIHelper.SystemMessage(LesCodeMsgHelper.GetItem("Disconnected"));//"网络断开。。。"
            LesLoadingHelper.LoadScene((int)ESceneDefultID.Login);
            return;
        }
        LesUIHelper.SystemMessage(LesCodeMsgHelper.GetItem("DisconnectedRelink"));//"网络断开重连中。。。"

        Relogin((err, player_data) =>
        {
            string error = LesUIHelper.Translate(err);
            if (!string.IsNullOrEmpty(error))
            {
                LesUIHelper.SystemMessage(LesCodeMsgHelper.GetItem("RelinkFail") + error);//"重连失败！ "
                LesLoadingHelper.LoadScene((int)ESceneDefultID.Login);
                return;
            }
            LesUIHelper.SystemMessage(LesCodeMsgHelper.GetItem("RelinkSuccess"));//"重连成功！"
            LesPlayerDataManager.Instance.PlayerData = player_data;
#if (UNITY_EDITOR || UNITY_STANDALONE_WIN)
            if(LesRobot.Instance)
            {
                LesRobot.Instance.OnReConnected();
            }
#endif
        });
    }
}
