using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

class SocketHelper
{
    /// <summary>
    /// 解析主机端口到IPEndPoint
    /// </summary>
    /// <param name="host">主机地址，IP，域名</param>
    /// <param name="port">目标端口</param>
    /// <returns>返回IPEndPoint</returns>
    public static IPEndPoint ParseAddress(string host, int port)
    {
        IPEndPoint enpoint = null;

        try
        {
            //enpoint = new IPEndPoint(IPAddress.Parse(host), port);
            enpoint = GetEndPoint(host, port, Application.platform);
        }
        catch (Exception)
        {
        }

        if (enpoint == null)
        {
            try
            {
                IPAddress[] hostAddresses = Dns.GetHostAddresses(host);

                if (hostAddresses.Length == 0)
                {
                    throw new ArgumentException("Unable to retrieve address from specified host name.", host);
                }

                enpoint = new IPEndPoint(hostAddresses[0], port);
            }
            catch (Exception e)
            {
                throw new ArgumentException(e.Message);
            }
        }

        if (enpoint == null)
        {
            throw new ArgumentException("Unable to create retrieve IPEndPoint from specified host name.", host);
        }

        return enpoint;
    }

    public const uint HEADER_MASK = 0xFFFF0000;
    public const uint LENGTH_MASK = 0x0000FFFF;

    public const uint HEAD_CRY_MASK = 0x80000000; // 加密选项
    public const uint HEAD_CMP_MASK = 0x40000000; // 压缩选项
    public const uint HEAD_CHK_MASK = 0x20000000; // 校验码
    public const uint HEAD_SEQ_MASK = 0x10000000; // 序列ID
    public const uint HEAD_REQ_MASK = 0x08000000; // 请求ID
    public const uint HEAD_MSG_MASK = 0x04000000; // 消息类型
    public const uint HEAD_RPC_MASK = 0x02000000; // RPC调用选项
    public const uint HEAD_UDP_INT = 0x01000000;
    public const uint HEAD_UDP_REP = 0x00800000;
    public const uint HEAD_UDP_ACK = 0x00400000;

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

        if (CheckMask(head, HEAD_CHK_MASK))
        {
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

    public static uint readUint(int offset, byte[] bytes)
    {
        uint result = 0;

        result |= (uint)(bytes[offset + 0] << 0);
        result |= (uint)(bytes[offset + 1] << 8);
        result |= (uint)(bytes[offset + 2] << 16);
        result |= (uint)(bytes[offset + 3] << 24);

        return result;
    }

    public static byte[] writeBytes(byte[] source, int start, int length, byte[] target)
    {
        return writeBytes(source, start, length, 0, target);
    }

    public static byte[] writeBytes(byte[] source, int start, int length, int offset, byte[] target)
    {
        for (int i = 0; i < length; i++)
        {
            target[offset + i] = source[start + i];
        }
        return target;
    }

    public static IPEndPoint GetEndPoint(string ip, int port, RuntimePlatform platform = RuntimePlatform.Android)
    {
        if (string.IsNullOrEmpty(ip) == true)
        {
            return null;
        }

        if (platform != RuntimePlatform.IPhonePlayer)
        {
            return new IPEndPoint(IPAddress.Parse(ip), port);
        }
        Debug.Log("=============Test getIPAddressIOS===============");
        string rtn = getIPAddressIOS(ip);
        if (string.IsNullOrEmpty(rtn) == true)
        {
            return new IPEndPoint(IPAddress.Parse(ip), port);
        }
        Debug.Log("Test getIPAddressIOS: " + rtn);
        string[] ips = rtn.Split(',');
        if (ips == null || ips.Length < 2)
        {
            return new IPEndPoint(IPAddress.Parse(ip), port);
        }
        if (ips[1].Equals("ipv6"))
        {
            Debug.Log("Test getIPAddressIOS ips[0]: " + ips[0]);
            return new IPEndPoint(IPAddress.Parse(ips[0]), port);
        }

        return new IPEndPoint(IPAddress.Parse(ip), port);
    }

    [DllImport("__Internal")]
    private static extern string getIPAddressIOS(string mHost);
}
