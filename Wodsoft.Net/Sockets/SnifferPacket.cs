using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Wodsoft.Net.Sockets
{
    public class SnifferPacket
    {
        private int HeadLength;

        public SnifferPacket(byte[] rawData)
        {
            if (rawData.Length < 20)
                throw new ArgumentException("无效的包数据。");
            if (rawData[2] * 256 + rawData[3] != rawData.Length)
                throw new ArgumentException("无效的包数据。");
            Time = DateTime.Now;
            RawData = rawData;
            HeadLength = (rawData[0] & 0x0F) * 4;
        }

        public DateTime Time { get; private set; }

        public byte[] RawData { get; private set; }

        public IPEndPoint Source
        {
            get
            {
                IPEndPoint ip = new IPEndPoint( new IPAddress(BitConverter.ToUInt32(RawData, 12)),0);
                if (Protocol == Sockets.Protocol.Tcp || Protocol == Sockets.Protocol.Udp)
                    ip.Port = RawData[HeadLength] * 256 + RawData[HeadLength + 1];
                return ip;
            }
        }

        public IPEndPoint Destination
        {
            get
            {
                IPEndPoint ip = new IPEndPoint(new IPAddress(BitConverter.ToUInt32(RawData, 16)), 0);
                if (Protocol == Sockets.Protocol.Tcp || Protocol == Sockets.Protocol.Udp)
                    ip.Port = RawData[HeadLength + 2] * 256 + RawData[HeadLength + 3];
                return ip;
            }
        }
        
        public Protocol Protocol
        {
            get
            {
                if (Enum.IsDefined(typeof(Protocol), (int)RawData[9]))
                    return (Protocol)RawData[9];
                else
                    return Protocol.Other;
            }
        }

        public byte[] Data
        {
            get
            {
                return RawData.Skip(HeadLength + 8).ToArray();
            }
        }

        public int Version
        {
            get
            {
                return (RawData[0] & 0xF0) >> 4;
            }
        }
    }
}
