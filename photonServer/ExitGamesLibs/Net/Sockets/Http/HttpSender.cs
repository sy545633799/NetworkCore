using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;

namespace ExitGames.Net.Sockets.Http
{
    public class HttpSender : IDisposable, ISocketSender
    {
        // Fields
        private long bytesSent;
        public readonly string Address;
        public readonly int TimeoutMilliseconds;

        // Methods
        public HttpSender(string address)
        {
            Address = address;
            TimeoutMilliseconds = 15000;
        }

        public HttpSender(string address, int timeoutMilliseconds)
        {
            Address = address;
            TimeoutMilliseconds = timeoutMilliseconds;
        }

        public void Dispose()
        {
        }

        public void Send(byte[] data)
        {
            this.Send(data, 0, data.Length);
        }

        public void Send(IList<ArraySegment<byte>> data)
        {
            string address = Address;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(address);
            request.Method = "POST";
            request.Timeout = TimeoutMilliseconds;
            Stream stream = request.GetRequestStream();
            long num = 0L;
            for (int i = 0; i < data.Count; i++)
            {
                ArraySegment<byte> segment = data[i];
                ArraySegment<byte> segment2 = data[i];
                ArraySegment<byte> segment3 = data[i];
                stream.Write(segment.Array, segment2.Offset, segment3.Count);
                ArraySegment<byte> segment4 = data[i];
                num += segment4.Count;
            }
            stream.Close();
            stream.Dispose();
            WebResponse response = request.GetResponse();
            response.Close();
            Interlocked.Add(ref bytesSent, num);
        }

        public void Send(byte[] data, int offset, int length)
        {
            string address = Address;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(address);
            request.Method = "POST";
            request.Timeout = TimeoutMilliseconds;
            Stream stream = request.GetRequestStream();
            stream.Write(data, offset, length);
            stream.Close();
            stream.Dispose();
            WebResponse response = request.GetResponse();
            response.Close();
            Interlocked.Add(ref bytesSent, length);
        }

        // Properties
        public bool Connected
        {
            get
            {
                return true;
            }
        }

        public IPEndPoint EndPoint
        {
            get
            {
                return new IPEndPoint(IPAddress.Any, 80);
            }
        }

        public long TotalBytesSent
        {
            get
            {
                return bytesSent;
            }
        }
    }
}
