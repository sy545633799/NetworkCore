using Mono.Security.Cryptography;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Timers;

/// <summary>
/// TcpClientWorker
/// </summary>
public class TcpClientWorker
{
    private static readonly byte[] KEEPALIVE_REQ_PACKET = new byte[] { 0, 0, 0, 16, 1, 0, 0, 0 };
    private static readonly byte[] KEEPALIVE_ACK_PACKET = new byte[] { 0, 0, 0, 16, 2, 0, 0, 0 };
    private static readonly byte[] HANDSHAKE_REQ_PACKET = new byte[] { 0, 0, 0, 16, 3, 0, 0, 0 };
    private static readonly byte[] HANDSHAKE_ACK_PACKET = new byte[] { 0, 0, 0, 16, 4, 0, 0, 0 };

    private const int MAX_BUFFER_SIZE = 65535;  // buffer 长度
    private const int HEAD_LENGTH = 4;          // 头部长度
    private const float KEEP_ALIVE_TIMEOUT = 5; // 超时时间
    private const int KEEP_ALIVE_COUNT = 3;     // 超时次数

    // 发送 & 接受队列
    private Queue<byte[]> m_sendQueue = new Queue<byte[]>();
    private Queue<Packet> m_recvQueue = new Queue<Packet>();

    // 发送 & 接受队列 锁对象
    private readonly object m_sendQueueLocker = new object();
    private readonly object m_recvQueueLocker = new object();
    private readonly object m_socketLocker = new object();
    
    // 发送 & 接受线程
    private Thread m_sendThread;
    private Thread m_recvThread;

    // 接受 & 发送缓冲
    private byte[] m_sendBuffer;
    private byte[] m_recvBuffer;
    private int m_recvBufferOffset;

    private byte[] mCryptoKey = null;
    RC4Cipher mEncoder = null;
    private bool mEnableCrypto = false;

    // socket
    private Socket m_socket;
    private IPEndPoint m_endPoint;
    private volatile bool m_checkworking = true;

    private volatile int m_keepAliveCount = KEEP_ALIVE_COUNT;
    private volatile float m_realTime;
    private volatile float m_lastTime;
    private volatile bool m_connecting;

    public bool Connected { get { return m_socket != null && m_socket.Connected; } }
    public bool Connecting { private set { m_connecting = value; } get { return m_connecting; } }
    public bool EnableCrypto { get { return mEnableCrypto; } }
    public RC4Cipher Encoder { get { return mEncoder; } }

    public TcpClientWorker()
    {
        m_sendBuffer = new byte[MAX_BUFFER_SIZE];
        m_recvBuffer = new byte[MAX_BUFFER_SIZE];
    }

    /// <summary>
    /// 连接指定的服务器
    /// </summary>
    /// <param name="host"></param>
    /// <param name="port"></param>
    public bool Connect( string host, int port )
    {
        lock (m_socketLocker)
        {
            if (null != m_socket && m_socket.Connected)
                throw new Exception("Exception. the tcpClient has Connectted.");

            try
            {
                // 解析对端地址
                m_endPoint = SocketHelper.ParseAddress(host, port);
                var family = AddressFamily.InterNetwork;
                if (AddressFamily.InterNetworkV6 == m_endPoint.AddressFamily)
                    family = AddressFamily.InterNetworkV6;
                // 创建一个新的socket用于连接
                m_socket = new Socket(family, SocketType.Stream, ProtocolType.Tcp);
                m_socket.ReceiveBufferSize = 1024;// MAX_BUFFER_SIZE;
                m_socket.NoDelay = true;

                UnityEngine.Debug.Log("Connect: " + m_endPoint.AddressFamily + "  " + m_endPoint.ToString());
                m_socket.Connect(m_endPoint);

                // 连接成功 启动接受线程开始接受数据
                putSequence(NetHelper.SEQUENCE_CONNECT, string.Empty);
                StartWorker();
            }
            catch(Exception e)
            {
                putSequence(NetHelper.SEQUENCE_CONNECT, e.Message);
                UnityEngine.Debug.Log("Exception. the tcpClient do connecting error." + e.Message);
                return false;
            }
        }

        return true;
    }

    // 由于Unity3d下的异步函数实现不稳定，此处异步连接可能无法正常工作
    public void AsyncConnect(string host, int port)
    {
        lock (m_socketLocker)
        {
            if (null != m_socket && m_socket.Connected)
                throw new Exception("Exception. the tcpClient has Connectted.");

            Connecting = true;

            try
            {
                m_endPoint = SocketHelper.ParseAddress(host, port);
                var family = AddressFamily.InterNetwork;
                if (AddressFamily.InterNetworkV6 == m_endPoint.AddressFamily)
                    family = AddressFamily.InterNetworkV6;
                m_socket = new Socket(family, SocketType.Stream, ProtocolType.Tcp);
                m_socket.ReceiveBufferSize = MAX_BUFFER_SIZE;
                m_socket.NoDelay = true;
            }
            catch (Exception e)
            {
                Connecting = false;
                putSequence(NetHelper.SEQUENCE_CONNECT, "AsyncConnect: " + e.Message);
                return;
            }

#if UNITY_EDITOR
            UnityEngine.Debug.Log("BeginConnect: " + m_endPoint.AddressFamily + "  " + m_endPoint.ToString());
#endif
            m_socket.BeginConnect(m_endPoint, (IAsyncResult ar) => 
            {
                try
                {
                    // 确认连接远程服务器
                    m_socket.EndConnect(ar);
                    // 连接成功 启动接受线程开始接受数据
                    putSequence(NetHelper.SEQUENCE_CONNECT, string.Empty);
                    StartWorker();
                }
                catch (Exception e)
                {
                    putSequence(NetHelper.SEQUENCE_CONNECT, "AsyncConnect BeginConnect: " + e.Message);
                }

                Connecting = false;
            }, m_socket);
        }
    }

    public void Close(string reason)
    {
        lock (m_socketLocker)
        {
            m_checkworking = false;
            if (m_socket != null)
            {
                try
                {
                    m_socket.Close();
                }
                catch (Exception e) {
                    UnityEngine.Debug.Log("socket.close: " + e.Message);
                }
                m_socket = null;
                mCryptoKey = null;
                mEnableCrypto = false;

                UnityEngine.Debug.Log("close reason: " + reason);
                // 通知应用层，连接关闭了
                putSequence(NetHelper.SEQUENCE_KICK, reason);
            }
        }
    }

    public Packet Recv()
    {
        m_realTime = UnityEngine.Time.realtimeSinceStartup;

        lock (m_recvQueueLocker)
        {
            if (m_recvQueue.Count == 0)
                return null;

            return m_recvQueue.Dequeue();
        }
    }

    public void Send(byte[] packet)
    {
        lock (m_sendQueueLocker)
        {
            m_sendQueue.Enqueue(packet);
        }
    }

    private void StartWorker()
    {
        StopWorker();

        m_checkworking = true;
        m_lastTime = m_realTime;
        m_keepAliveCount = KEEP_ALIVE_COUNT;

        if (m_recvThread == null)
        {
            m_recvThread = new Thread(new ThreadStart(DoReceive));
            m_recvThread.IsBackground = true;
        }

        if (m_sendThread == null)
        {
            m_sendThread = new Thread(new ThreadStart(BusySend));
            m_sendThread.IsBackground = true;
        }

        if (!m_recvThread.IsAlive)
            m_recvThread.Start();

        if (!m_sendThread.IsAlive)
            m_sendThread.Start();
    }

    private void StopWorker()
    {
        m_checkworking = false;

        if (m_recvThread != null)
            m_recvThread = null;

        if (m_sendThread != null)
            m_sendThread = null;
    }

    private void checkKeepAlive()
    {
        if (0 == m_realTime || (m_socket == null || m_socket.Connected == false)) return;

        if (m_realTime - m_lastTime < KEEP_ALIVE_TIMEOUT) return;

        if (m_keepAliveCount > 0)
        {
            m_keepAliveCount -= 1;
            Send(KEEPALIVE_REQ_PACKET);
            m_lastTime = m_realTime;
        }
        else {
            Close("keepAliveTimeout");
        }
    }

    private void BusySend()
    {
        while (m_checkworking)
        {
            // 检查连接是否存活
            checkKeepAlive();

            // 检查发送任务 & 发送之, 如果没有任何包被发送那么延迟一下
            if (!DoSend() ) Thread.Sleep(10);

        }
    }

    private void DoReceive()
    {
        do
        {
            var bytesRead = 0; // 当前已经收取的字节数
            var capcity = MAX_BUFFER_SIZE - m_recvBufferOffset; // 当前剩余可用空间

            if (capcity <= 0)
            {
                UnityEngine.Debug.Log("m_recvBuffer not enough.");
                Thread.Sleep(1000);
            }
            else
            {
                try
                {
                    // 开始从对端接受数据到缓存区里面
                    bytesRead = m_socket.Receive(m_recvBuffer, m_recvBufferOffset, capcity, SocketFlags.None);
                    m_recvBufferOffset += bytesRead;
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.Log("m_socket.Receive, exception: " + ex.Message);
                    Close("m_socket.Receive: " + ex.Message);
                }
            }

            // 开始分包
            SplitPackets();
        }
        while (m_checkworking);
    }

    private void SplitPackets()
    {
        var m_readPos = 0;
        while ((m_recvBufferOffset - m_readPos) > HEAD_LENGTH)
        {
            //Get package length
            var pkgLength = NetHelper.PacketSize(SocketHelper.readUint(m_readPos, m_recvBuffer ));

            // 剩下的数据不够一个完整的包则放弃解析
            if (pkgLength > (m_recvBufferOffset - m_readPos))
                break;

            var packet = new byte[pkgLength];
            Buffer.BlockCopy(m_recvBuffer, m_readPos, packet, 0, pkgLength);
            processPacket(packet);

            m_readPos += pkgLength;
        }

        // 还有没有读取完毕的 往前挪一下
        if (m_readPos > 0 && m_readPos < m_recvBufferOffset)
            Buffer.BlockCopy(m_recvBuffer, m_readPos, m_recvBuffer, 0, m_recvBufferOffset - m_readPos);

        // 伸缩
        m_recvBufferOffset -= m_readPos;
    }

    private void processPacket(byte[] bytes)
    {
        var packet = Packet.ParsePacket(bytes, mCryptoKey);

        if (packet.CheckMask(NetHelper.HEAD_SEQ_MASK))
        {
            this.OnSequence(packet.Sequence());
        }

        // 应用层消息入队列
        if (packet.CheckMask(NetHelper.HEAD_MSG_MASK))
        {
            m_lastTime = m_realTime;
            m_keepAliveCount = KEEP_ALIVE_COUNT;

            if (packet.MsgType() == (uint)ERequestTypes.EResponse)
            {
                var response = packet.Parse<Response>();
                if (response.mtype == (int)ERequestTypes.ERequestSyncTimeStampCmd)
                {
                    var resp = response.Parse<RequestSyncTimeStampCmd>();
                    LesTimeHelper.UpdateServerTime(resp.server, LesTimeHelper.LocalMSecond() - resp.client);
                    return;
                }
            }

            lock (m_recvQueueLocker)
            {
                //UnityEngine.Debug.Log("processPacket: " + packet.Name());
                m_recvQueue.Enqueue(packet);
            }
        }
    }

    private void OnSequence(UInt32 sequence)
    {
        switch (sequence)
        {
            case NetHelper.SEQUENCE_HANDSHAKE:
                Send(HANDSHAKE_ACK_PACKET);
                break;
            case NetHelper.SEQUENCE_HANDSHAKE_ACK:
                mEnableCrypto = true;
                putSequence(NetHelper.SEQUENCE_CONNECT, null);
                break;
            case NetHelper.SEQUENCE_KEEPALIVE:
                Send(KEEPALIVE_ACK_PACKET);
                break;
            case NetHelper.SEQUENCE_KEEPALIVE_ACK:
                m_lastTime = m_realTime;
                m_keepAliveCount = KEEP_ALIVE_COUNT;
                break;
            case NetHelper.SEQUENCE_KICK:
                Close("ServerKick");
                break;
            default:
                break;
        }
    }

    private void putSequence(UInt32 sequnece, string extra)
    {
        lock (m_recvQueueLocker)
        {
            // 如果连接成功那么发送加密码
            //if (NetHelper.SEQUENCE_CONNECT == sequnece && string.IsNullOrEmpty(extra) && null == mCryptoKey)
            //{
            //    var seed = new System.Random().Next(0, Int32.MaxValue);
            //    mCryptoKey = RC4Utility.GenRC4SecretKey(seed);
            //    mEncoder = new RC4Cipher(mCryptoKey);
            //    var handleshake = Packet.BuildPacket(null, NetHelper.SEQUENCE_HANDSHAKE, (uint)seed, 0, false, false);
            //    Send(handleshake.Build());
            //    return;
            //}

            var packet = Packet.BuildPacket(null, sequnece, 0, 0, false, false);
            packet.Extra = extra;
            m_recvQueue.Enqueue(packet);
        }
    }

    private bool DoSend()
    {
        lock (m_socketLocker)
        {
            if ((m_socket == null) || (m_socket.Connected == false))
                return false;
        }

        lock (m_sendQueueLocker)
        {
            // 木有待发送的任务
            if (m_sendQueue.Count == 0)
                return false;

            var TotalLength = 0;

            while (MAX_BUFFER_SIZE - TotalLength > 0 && m_sendQueue.Count > 0)
            {
                // 偷偷看一下这个包是不是还放的下
                var packet = m_sendQueue.Peek();

                // 缓冲区空间不足, 则跳过这次合并
                if (!(TotalLength + packet.Length < MAX_BUFFER_SIZE))
                    break;

                // 放得下就合并掉一起发送
                packet.CopyTo(m_sendBuffer, TotalLength);
                TotalLength += packet.Length;

                // 弹出这个已经合并的包
                m_sendQueue.Dequeue();
            }

            try
            {
                SocketError error;
                // 发送合并之后的包
                m_socket.Send(m_sendBuffer, 0, TotalLength, SocketFlags.None, out error);
                if (SocketError.Success != error)
                    UnityEngine.Debug.LogError("Send: " + error);
                // 清空发送缓存 (非必要)
                System.Array.Clear(m_sendBuffer, 0, MAX_BUFFER_SIZE);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log("stream write error :" + e.Message);
                Close("m_socket.Send: " + e.Message);
            }
        }

        return true;
    }

}

