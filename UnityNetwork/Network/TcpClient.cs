using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

public class TcpClient
{
    public TcpClientWorker m_tcpWorker = new TcpClientWorker();
    private EventManager mEventManager = new EventManager();

    private UInt32 requestId = 1;
    private UInt32 sequenceId = NetHelper.SEQUENCE_INTERNAL;

#if UNITY_EDITOR
    // for robot.
    private bool mEnableRecord = false;
    private Int64 mStartTime = 0;
    private MessageList mMessageList;

    private Dictionary<string, Int32> mCmdStat = new Dictionary<string, int>();
    public float mLastResetTime;
#endif

    private Action<string> OnConnectHandler;
    public Action<string> OnCloseHandler;

    public bool Connected { get { return m_tcpWorker.Connected; } }
    public bool Connecting { get { return m_tcpWorker.Connecting; } }

    public TcpClient()
    {
    }

    private void send(UInt32 requestid, RequestCmd cmd)
    {
        var packet = Packet.BuildPacket(cmd, ++sequenceId, requestid, 0, false, m_tcpWorker.EnableCrypto);
        var bytes = packet.Build(m_tcpWorker.Encoder);

        m_tcpWorker.Send(bytes);
    }

    public void connect(string host, int port, Action<string> handler)
    {
        if (Connecting) return;
#if UNITY_EDITOR
        UnityEngine.Debug.Log("connecting..." + host + ":" + port);
#endif

        try
        {
            OnConnectHandler = handler;
            //m_tcpWorker.Connect(host, port);
            m_tcpWorker.AsyncConnect(host, port);
        }
        catch (Exception e)
        {
            UnityEngine.Debug.Log("连接服务器失败: " + host + ":" + port + " " + e.Message);
        }
    }

    public void request(RequestCmd msg, Action<string, Response> action)
    {
        this.mEventManager.AddCallBack(requestId, action);
        send(requestId, msg);
        requestId++;
    }

    // 注册指定服务器推送消息
    public void Register<T>(Action<T> action) where T : ICommand
    {
        mEventManager.AddOnEvent<T>(action);
    }

    // 注册指定服务器推送消息
    public void Register(ERequestTypes eventId, Action<ICommand> action)
    {
        mEventManager.AddOnEvent(eventId, action);
    }

    public void close()
    {
        OnCloseHandler = null;
        m_tcpWorker.Close("ClientEOF");
    }

    public bool ProcessPacket()
    {
        var packet = m_tcpWorker.Recv();
        if (null == packet) return false;

#if UNITY_EDITOR

        //if (0 == mLastResetTime || (UnityEngine.Time.realtimeSinceStartup - mLastResetTime) > 1f)
        //{
        //    mLastResetTime = UnityEngine.Time.realtimeSinceStartup;
        //    //mCmdStat.Clear();
        //}

        //if (packet.MsgType() == (uint)ERequestTypes.ESyncExcuteActionCmd)
        //{
        //    var cmd = packet.GetCommand() as SyncExcuteActionCmd;

        //    if (cmd.type == LesActionType.DoAction)
        //    {
        //        var name = packet.Name() + "-" + cmd.type + "-" + (LesUnitState)cmd.args[0];
        //        if (!string.IsNullOrEmpty(name))
        //        {
        //            if (!mCmdStat.ContainsKey(name))
        //                mCmdStat.Add(name, 0);
        //            mCmdStat[name] += 1;
        //        }
        //    }
        //}

        //NGUIDebug.Clear();
        //foreach(var pair in mCmdStat)
        //    NGUIDebug.Log(pair.Key + " -> " + pair.Value);
#endif

        // handle response message.
        if (packet.MsgType() == (uint)ERequestTypes.EResponse)
        {
            var response = packet.Parse<Response>();
            mEventManager.InvokeCallBack(response.id, response);
            return true;
        }

        switch (packet.Sequence())
        {
            // on sync connect result.
            case NetHelper.SEQUENCE_CONNECT:
                UnityEngine.Debug.Log("SEQUENCE_CONNECT: " + packet.Extra);
                OnConnectHandler(packet.Extra);
                break;
            // on connection closed.
            case NetHelper.SEQUENCE_KICK:
                UnityEngine.Debug.Log("SEQUENCE_KICK: " + packet.Extra);
                if (null != OnCloseHandler) OnCloseHandler(packet.Extra);
                break;
            // on connection handshake.
            case NetHelper.SEQUENCE_HANDSHAKE_ACK:
                break;
            // handle server push messages.
            default:
                var cmd = packet.GetCommand();
                if (cmd != null)
                {
                    mEventManager.InvokeOnEvent((ERequestTypes)packet.MsgType(), cmd);
                }
                //UnityEngine.Debug.Log("SendCmd ------>" + packet.GetCommand().GetType().Name + "  " + packet.AllSize() + " bytes");
                break;
        }

        return true;
    }
}
