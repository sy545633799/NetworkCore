using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ExitGames.Net.Sockets.Udp
{
    /// <summary>
    /// An <see cref="T:ExitGames.Net.Sockets.ISocketSender"/> implementation for UDP.
    /// </summary>
    public sealed class UdpSender : IDisposable, ISocketSender
    {
        /// <summary>
        /// The used <see cref="T:System.Net.Sockets.UdpClient"/>.
        /// </summary>
        private readonly UdpClient udpClient;

        /// <summary>
        /// The total number of bytes sent.
        /// </summary>
        private long bytesSent;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ExitGames.Net.Sockets.Udp.UdpSender"/> class.
        /// </summary>
        /// <param name="endPoint">The end point.</param>
        public UdpSender(IPEndPoint endPoint)
        {
            udpClient = new UdpClient();
            EndPoint = endPoint;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ExitGames.Net.Sockets.Udp.UdpSender"/> class.
        /// </summary>
        /// <param name="address">The IP address.</param>
        /// <param name="port">The port.</param>
        public UdpSender(IPAddress address, int port)
            : this(new IPEndPoint(address, port))
        {
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="T:ExitGames.Net.Sockets.Udp.UdpSender"/> class.
        /// </summary>     
        ~UdpSender()
        {
            Dispose(false);
        }

        /// <summary>
        /// Connects to the <see cref="P:ExitGames.Net.Sockets.Udp.UdpSender.EndPoint"/>.
        /// </summary>
        public void Start()
        {
            udpClient.Connect(EndPoint);
        }

        /// <summary>
        /// Disposes all managed and unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Sends a byte array.
        /// </summary>
        /// <param name="data">The data.</param>
        public void Send(byte[] data)
        {
            int num;

            lock (udpClient)
            {
                num = udpClient.Send(data, data.Length);
            }
            Interlocked.Add(ref bytesSent, num);
        }

        /// <summary>
        /// Sends a byte array.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        public void Send(byte[] data, int offset, int length)
        {
            int num;
            lock (udpClient)
            {
                num = udpClient.Client.Send(data, offset, length, SocketFlags.None);
            }
            Interlocked.Add(ref bytesSent, num);
        }

        /// <summary>
        /// Sends a list of <see cref="T:System.ArraySegment`1"/> of byte.
        /// </summary>
        /// <param name="data">The data.</param>
        public void Send(IList<ArraySegment<byte>> data)
        {
            int num;
            lock (udpClient)
            {
                num = udpClient.Client.Send(data);
            }
            Interlocked.Add(ref bytesSent, num);
        }

        /// <summary>
        /// Closes the <see cref="T:System.Net.Sockets.UdpClient"/>.
        /// </summary>
        /// <param name="disposing">True if called from <see cref="M:ExitGames.Net.Sockets.Udp.UdpSender.Dispose"/>.</param>
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                udpClient.Close();
            }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:ExitGames.Net.Sockets.ISocketSender"/> is connected 
        /// to a remote host as of the last Send operation.
        /// </summary>
        public bool Connected
        {
            get
            {
                return udpClient != null && udpClient.Client.Connected;
            }
        }

        /// <summary>
        /// Gets the endpoint address to which the underling socket is associated.
        /// </summary>
        public IPEndPoint EndPoint { get; private set; }

        /// <summary>
        /// Gets the number of bytes sent by this instance.
        /// </summary>
        public long TotalBytesSent
        {
            get
            {
                return bytesSent;
            }
        }
    }
}
