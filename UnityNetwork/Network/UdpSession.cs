using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class UdpSession_v2 {

    private UdpSocket mSocket;
    private EventManager mEvManager = new EventManager();
    private UInt32 mSequence;
    private UInt32 mRequestId;

    private readonly byte[] REQ_DATA = new byte[8] { 0, 0, 0, 16, 1, 0, 0, 0 };
    private readonly byte[] ACK_DATA = new byte[8] { 0, 0, 0, 16, 2, 0, 0, 0 };
    private const UInt32 RESEND_TIME = 500;
    private const UInt32 KEEP_ALIVE_TIME = 1000 * 10;

    private UInt32 mKeepAliveCount = 10;
    private UInt32 mLastRecvTime = 0;
    private UInt32 mLastSendTime = 0;
    private UInt32 IncSequence { set{} get { return ++mSequence; } }
    private UInt32 IncRequestId { set{} get { return ++mRequestId; } }

    public Action<bool, string> ConnectHandler;
    public Action<string> ClosedHandler;

    public UdpSession_v2() 
    { 
    }

    public void Connect(string host, UInt16 port)
    {
        mSocket = new UdpSocket(OnEventHandler);

        mSocket.Connect(host, port);
    }

    void OnEventHandler(UdpSocket.cliEvent ev, byte[] buf, string err)
    {
        switch (ev) { 
            case UdpSocket.cliEvent.Connected:
                ConnectHandler(true, err);
                break;
            case UdpSocket.cliEvent.ConnectFailed:
                ConnectHandler(false, err);
                break;
            case UdpSocket.cliEvent.Disconnect:
                ClosedHandler(err);
                break;
            case UdpSocket.cliEvent.RcvMsg:
                OnDataReceived(buf);
                break;
            default:
                throw new Exception("unknown event.");
        }
    }

    void OnDataReceived(byte[] data)
    {
        int dataLen = data.Length;
        if (dataLen < 4)
            return;

        var packet = Packet.ParsePacket(data);

        if (null == packet) return;

        // directly reply ack request, donot use the Send method as we in another thread.
        if (packet.CheckMask(NetHelper.HEAD_SEQ_MASK))
        {
            // process internal sequence.
            OnCheckSequence(packet.Sequence());

            // internal sequence.
            if (packet.Sequence() < NetHelper.SEQUENCE_INTERNAL)
            {
                return;
            }
        }

        // push message packet to queue.
        if (packet.CheckMask(NetHelper.HEAD_MSG_MASK))
        {
            mLastRecvTime = UdpSocket.iclock();
            OnDataPacket(packet);
        }
    }

    void OnCheckSequence(UInt32 seq)
    {
        switch (seq)
        {
            case NetHelper.SEQUENCE_KICK:
                Close("ServerClosed");
                break;
            case NetHelper.SEQUENCE_KEEPALIVE:
                mSocket.Send(ACK_DATA);
                break;
            case NetHelper.SEQUENCE_KEEPALIVE_ACK:
                mKeepAliveCount = 10;
                mLastRecvTime = UdpSocket.iclock();
                break;
        }
    }

    void DoKeepaliveInLoop(UInt32 current)
    {
        // 关闭中的不用管
        if (mSocket.InCloseStage) return;

        // 未超时的不用管
        if (current < mLastRecvTime + KEEP_ALIVE_TIME) return;

        // 继续等待返回
        if (current < mLastSendTime + RESEND_TIME) return;

        if (mKeepAliveCount > 0)
        {
            mKeepAliveCount -= 1;
            mLastSendTime = current;
            mSocket.Send(REQ_DATA);
            return;
        }

        this.Close("KeepAliveTimeout");
    }

    void OnDataPacket(Packet packet)
    {
        switch (packet.MsgType()) {
            // handle response message.
            case (uint)ERequestTypes.EResponse:
                {
                    var response = packet.Parse<Response>();
                    mEvManager.InvokeCallBack(response.id, response);
                    break;
                }
            // handle server push message.
            default:
                {
                    var cmd = packet.GetCommand();

                    if (cmd != null)
                        mEvManager.InvokeOnEvent((ERequestTypes)packet.MsgType(), cmd);
                    break;
                }
        }
    }

    public void Update()
    {
        if (mSocket != null)
        {
            var current = UdpSocket.iclock();
            // update kcp-control.
            mSocket.Update(current);
            // check keep-alive.
            DoKeepaliveInLoop(current);
        }
    }

    public void Close(string reason)
    {
        if (mSocket != null)
            mSocket.Close(reason);
    }

    void Send(Packet packet)
    {
        var bytes = packet.Build();
        mSocket.Send(bytes);
    }

    public void Register<T>(Action<T> action) where T : ICommand
    {
        mEvManager.AddOnEvent<T>(action);
    }

    public void Register(ERequestTypes eventId, Action<ICommand> action)
    {
        mEvManager.AddOnEvent(eventId, action);
    }

    public void Request(ICommand cmd, Action<string, Response> handler)
    {
        var packet = Packet.BuildPacket(cmd, 0, IncRequestId, 0, false, false);
        if (handler != null) mEvManager.AddCallBack(packet.RequestId(), handler);
        Send(packet);
    }
}