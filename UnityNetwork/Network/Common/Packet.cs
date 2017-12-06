using System.Collections;
using System;
using System.IO;

public static class NetHelper {

    public const uint HEAD_CRY_MASK = 0x80000000; // 加密选项
    public const uint HEAD_CMP_MASK = 0x40000000; // 压缩选项
    public const uint HEAD_CHK_MASK = 0x20000000; // 校验码
    public const uint HEAD_SEQ_MASK = 0x10000000; // 序列ID
    public const uint HEAD_REQ_MASK = 0x08000000; // 请求ID
    public const uint HEAD_MSG_MASK = 0x04000000; // 消息类型
    public const uint HEAD_RPC_MASK = 0x02000000; // RPC调用选项
    public const uint HEAD_UDP_INT  = 0x01000000;
    public const uint HEAD_UDP_REP  = 0x00800000;
    public const uint HEAD_UDP_ACK  = 0x00400000;


    public const uint HEADER_MASK = 0xFFFF0000;
    public const uint LENGTH_MASK = 0x0000FFFF;

    public const uint SEQUENCE_KEEPALIVE        = 1;
    public const uint SEQUENCE_KEEPALIVE_ACK    = 2;
    public const uint SEQUENCE_HANDSHAKE        = 3;
    public const uint SEQUENCE_HANDSHAKE_ACK    = 4;
    public const uint SEQUENCE_KICK             = 5;
    public const uint SEQUENCE_CONNECT          = 6;

    public const uint SEQUENCE_INTERNAL         = 100;

    public static bool CheckMask(UInt32 head, UInt32 mask)
    {
        return 0 != ((head & HEADER_MASK) & mask);
    }

    public static UInt16 MsgSize(UInt32 head) 
    {
        return (UInt16)(head & LENGTH_MASK);
    }

    public static UInt16 PacketSize(UInt32 head)
    {
        return (UInt16)(HeadSize(head) + MsgSize(head));
    }

    public static UInt16 HeadSize(UInt32 head)
    {
        UInt16 length = 4;

        if( CheckMask( head, HEAD_CHK_MASK ) ) {
            length += 4;
        }


        if (CheckMask(head, HEAD_SEQ_MASK))
        {
            length += 4;
        }


        if (CheckMask(head, HEAD_REQ_MASK))
        {
            length += 4;
        }


        if (CheckMask(head, HEAD_RPC_MASK))
        {
            length += 4;
        }


        if (CheckMask(head, HEAD_MSG_MASK))
        {
            length += 4;
        }

        return length;
    }

    public static UInt32 ComputeAdler32(byte[] data, int start, int length)
    {
        const int Base = 65521;
        const int NMax = 5552;
        const int initial = 1;

        UInt32 s1 = (uint)(initial & 0xffff);

        UInt32 s2 = (uint)((initial >> 16) & 0xffff);

        int index = start;
        int len = length;

        int k;
        while (len > 0)
        {
            k = (len < NMax) ? len : NMax;
            len -= k;

            for (int i = 0; i < k; i++)
            {
                s1 += data[index++];
                s2 += s1;
            }
            s1 %= Base;
            s2 %= Base;
        }

        return ((s2 << 16) | s1);
    }

    public static UInt32 Adler32(string str)
    {
        Byte[] data = System.Text.Encoding.UTF8.GetBytes(str);
        return ComputeAdler32(data, 0, data.Length);
    }

    public static UInt32 CRC32( string str )
    {
        return CRC.CRC32(str);
    }

    public static UInt32 BKDRHash(string str) {

        byte[] data = System.Text.Encoding.UTF8.GetBytes(str);

        // 31 131 1313 13131 131313 etc..
        UInt32 seed = 131;
        UInt32 hash = 0;

        for (var i = 0; i < data.Length; i++) {
            hash = hash * seed + data[i];
        }

        return hash & 0x7FFFFFFF;
    }

    public static string FindNameBy(UInt32 code)
    {
        string str = Enum.GetName(typeof( ERequestTypes ), code );

        return null != str ? str.Substring(1) : null;
    }

    public static Type FindMessageBy( UInt32 code )
    {
        Type ret = null;
        if (!RequestTypes.MessageMap.TryGetValue(code, out ret))
            UnityEngine.Debug.LogError("Can't find the message type: " + code);

        return ret;
    }
}

public class Packet {

     public const uint HEAD_SIZE = 4;

     public const uint MAX_MSG_LEN = 1 << 16 - 20;
     public const uint MAX_PKT_LEN = 1 << 16 - 1;

     public delegate void OnResponse(string err, Response response);

     UInt32 head;
     UInt32 checksum;
     UInt32 sequence;
     UInt32 requestId;
     UInt32 rpcId;
     UInt32 opcode;
     Byte[] message;

     public string Extra;

     // for udp.
     internal UInt32 tryCount;
     //internal Action<int> Callback;

     internal void setMask(UInt32 mask)
     {
         this.head |= (mask & NetHelper.HEADER_MASK);
     }

    /// <summary>
    /// 检查头部标志
    /// </summary>
    /// <param name="mask"></param>
    /// <returns></returns>
     public bool CheckMask(UInt32 mask)
     {
         return NetHelper.CheckMask(this.head, mask);
     }

    /// <summary>
    /// 获取消息名字
    /// </summary>
    /// <returns></returns>
     public string Name()
     {
         return NetHelper.FindNameBy(this.opcode);
     }

    /// <summary>
    /// 序列
    /// </summary>
    /// <returns></returns>
    public UInt32 Sequence() 
    {
        return this.sequence;
    }

    /// <summary>
    /// 请求ID
    /// </summary>
    /// <returns></returns>
    public UInt32 RequestId()
    {
        return this.requestId;
    }

    /// <summary>
    /// RPC调用ID
    /// </summary>
    /// <returns></returns>
    public UInt32 RpcId()
    {
        return this.rpcId;
    }

    /// <summary>
    /// 消息号
    /// </summary>
    /// <returns></returns>
    public UInt32 MsgType()
    {
        return this.opcode;
    }

    /// <summary>
    /// 消息大小
    /// </summary>
    /// <returns></returns>
    public UInt16 MsgSize()
    {
        return NetHelper.MsgSize(this.head);
    }

    /// <summary>
    /// 全部大小
    /// </summary>
    /// <returns></returns>
    public UInt16 AllSize()
    {
        return NetHelper.PacketSize( this.head );
    }

    public T Parse<T>()
    {
        return ProtoBuf.Serializer.Deserialize<T>(new MemoryStream(this.message));
    }

    public ICommand GetCommand() 
    {
        Type type = NetHelper.FindMessageBy(this.opcode);

        if (null == type)
        {
            UnityEngine.Debug.LogError("can't find the message: " + opcode);
            return null;
        }

        try
        {
            return ProtoBuf.Meta.RuntimeTypeModel.Default.Deserialize(new MemoryStream(this.message), null, type) as ICommand;
        }
        catch (Exception e)
        {
            //CrashReporter.Instance.ProcessExceptionReport(this.opcode.ToString() + " = " + type.Name + " => " + e.Message, e.StackTrace, UnityEngine.LogType.Exception);
            UnityEngine.Debug.LogException(e);
        }
        return null;
    }

    public static Packet BuildPacket(ICommand cmd, UInt32 sequence, UInt32 requestId, UInt32 rpcId, bool checksum, bool crypto)
    {
        Packet packet = new Packet();

        if (null != cmd)
        {
            MemoryStream cmd_stream = new MemoryStream();
            ProtoBuf.Serializer.Serialize(cmd_stream, cmd);

            packet.message = cmd_stream.ToArray();

            UInt32 MsgType = NetHelper.BKDRHash(cmd.GetType().Name);
            UInt32 MsgLen = (UInt32)packet.message.Length;

            if (MsgLen >= MAX_MSG_LEN)
                throw new Exception("Message too Large!! " + MsgLen);

            packet.head |= MsgLen;
            packet.opcode = MsgType;
            packet.head |= NetHelper.HEAD_MSG_MASK;

            if (0 != rpcId)
            {
                packet.rpcId = rpcId;
                packet.head |= NetHelper.HEAD_RPC_MASK;
            }

            if (checksum)
            {
                packet.head |= NetHelper.HEAD_CHK_MASK;
            }

            if (crypto)
            {
                packet.head |= NetHelper.HEAD_CRY_MASK;
            }
        }

        if (0 != requestId)
        {
            packet.requestId = requestId;
            packet.head |= NetHelper.HEAD_REQ_MASK;
        }

        if (0 != sequence)
        {
            packet.sequence = sequence;
            packet.head |= NetHelper.HEAD_SEQ_MASK;
        }

        return packet;
    }

    public static Packet ParsePacket(Byte[] data)
    {
        return ParsePacket(data, null);
    }

    public static Packet ParsePacket(Byte[] data, byte[] crypto_key) 
    {
        Packet packet = new Packet();

        BinaryReader reader = new BinaryReader( new MemoryStream( data ) );

        packet.head = reader.ReadUInt32();

        var packet_size = packet.AllSize();

        var checksum_offset = HEAD_SIZE;

        if (packet.CheckMask(NetHelper.HEAD_CRY_MASK) && packet.AllSize() > HEAD_SIZE)
        {
            //byte[] decryptData = AESEncryptionUtility.Decrypt(reader.ReadBytes(packet_size - (int)HEAD_SIZE), AESEncryptionUtility.defaultKey, AESEncryptionUtility.commonIV);
            var decryptData = reader.ReadBytes(packet_size - (int)HEAD_SIZE);
            RC4Utility.XORKeyStream(ref decryptData, crypto_key);
            reader = new BinaryReader(new MemoryStream(decryptData));
            data = decryptData;

            // 前面已经过滤了headmask 所以要回退4个字节
            checksum_offset -= HEAD_SIZE;
        }

        if (packet.CheckMask(NetHelper.HEAD_CHK_MASK))
        {
            packet.checksum = reader.ReadUInt32();

            // 自己不算内
            checksum_offset += 4;

            UInt32 checksum = NetHelper.ComputeAdler32(data, (int)checksum_offset, packet_size - (int)checksum_offset);

            if (packet.checksum != checksum)
                throw new Exception( "CheckSum Fail. " + packet.checksum + " != " + checksum );
        }

        if (packet.CheckMask(NetHelper.HEAD_SEQ_MASK)) {
            packet.sequence = reader.ReadUInt32();
        }

        if (packet.CheckMask(NetHelper.HEAD_REQ_MASK)) {
            packet.requestId = reader.ReadUInt32();
        }

        if (packet.CheckMask(NetHelper.HEAD_RPC_MASK)) {
            packet.rpcId = reader.ReadUInt32();
        }

        if (packet.CheckMask(NetHelper.HEAD_MSG_MASK)) {
            packet.opcode = reader.ReadUInt32();
            packet.message = reader.ReadBytes((int)packet.MsgSize());
        }

        return packet;
    }

    public Byte[] Build()
    {
        return Build(null);
    }

    public Byte[] Build(RC4Cipher crypto)
    {
        MemoryStream packet_stream = new MemoryStream(AllSize());
        BinaryWriter writer = new BinaryWriter(packet_stream);

        writer.Write(this.head);

        var checksum_offset = HEAD_SIZE;

        if(CheckMask( NetHelper.HEAD_CHK_MASK) && this.message.Length > (int)HEAD_SIZE ) {
            writer.Write(0);

            checksum_offset += 4;
        }

        if (CheckMask(NetHelper.HEAD_SEQ_MASK)) {
            writer.Write( this.sequence );
        }

        if (CheckMask(NetHelper.HEAD_REQ_MASK)) {
            writer.Write(this.requestId );
        }

        if (CheckMask(NetHelper.HEAD_RPC_MASK)) {
            writer.Write(this.rpcId);
        }

        if (CheckMask(NetHelper.HEAD_MSG_MASK)) {
            writer.Write(this.opcode);
            writer.Write(this.message);
        }

        if (CheckMask(NetHelper.HEAD_CHK_MASK)) {
            writer.Seek( (int)HEAD_SIZE, SeekOrigin.Begin );

            Byte[] packet_data = packet_stream.ToArray();
            this.checksum = NetHelper.ComputeAdler32(packet_data, (int)checksum_offset, packet_data.Length - (int)checksum_offset);

            writer.Write(this.checksum);
        }

        byte[] bytes = packet_stream.ToArray();

        if (CheckMask(NetHelper.HEAD_CRY_MASK) && bytes.Length > (int)HEAD_SIZE) {

            crypto.XORKeyStream(bytes, (int)HEAD_SIZE, bytes, (int)HEAD_SIZE, bytes.Length - (int)HEAD_SIZE);

            return bytes;
        }

        return bytes;
    }
}
