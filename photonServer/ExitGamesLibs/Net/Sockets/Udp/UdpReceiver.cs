using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ExitGames.Net.Sockets.Udp
{
    /// <summary>
    /// An <see cref="T:ExitGames.Net.Sockets.ISocketReceiver"/> implementation for UDP.
    /// </summary>
    public sealed class UdpReceiver : IDisposable, ISocketReceiver
    {
        /// <summary>
        /// The used <see cref="T:System.Net.Sockets.UdpClient"/>.
        /// </summary>
        private readonly UdpClient udpClient;

        /// <summary>
        /// The total number of bytes received.
        /// </summary>
        private long bytesReceived;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ExitGames.Net.Sockets.Udp.UdpReceiver"/> class.
        /// </summary>
        /// <param name="endPoint">The IP end point.</param>
        public UdpReceiver(IPEndPoint endPoint)
        {
            udpClient = new UdpClient(endPoint);
            EndPoint = endPoint;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ExitGames.Net.Sockets.Udp.UdpReceiver"/> class.
        /// </summary>
        /// <param name="address">The IP address.</param>
        /// <param name="port">The port.</param>
        public UdpReceiver(IPAddress address, int port)
            : this(new IPEndPoint(address, port))
        {
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="T:ExitGames.Net.Sockets.Udp.UdpReceiver"/> class.
        /// </summary>
        ~UdpReceiver()
        {
            Dispose(false);
        }

        /// <summary>
        /// Starts the UDP receiver.
        /// </summary>
        public void Start()
        {
            udpClient.BeginReceive(new AsyncCallback(ReceiveCallback), null);
        }

        /// <summary>
        /// Closes the <see cref="T:System.Net.Sockets.UdpClient"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Closes the <see cref="T:System.Net.Sockets.UdpClient"/>.
        /// </summary>
        /// <param name="disposing">True if called from <see cref="M:ExitGames.Net.Sockets.Udp.UdpReceiver.Dispose"/>.</param>
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                udpClient.Close();
            }
        }

        /// <summary>
        /// Called when something was received.
        /// </summary>
        /// <param name="asyncResult">The async result.</param>
        private void ReceiveCallback(IAsyncResult asyncResult)
        {
            try
            {
                IPEndPoint endPoint = EndPoint;
                byte[] buffer = udpClient.EndReceive(asyncResult, ref endPoint);
                Interlocked.Add(ref bytesReceived, buffer.Length);

                if (Receive != null)
                {
                    Receive(this, new SocketReceiveEventArgs(endPoint, buffer, 0, buffer.Length));
                }
                udpClient.BeginReceive(new AsyncCallback(ReceiveCallback), null);
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (OutOfMemoryException)
            {
                throw;
            }
            catch (Exception exception)
            {
                if (Error != null)
                {
                    Error(this, exception);
                }
            }
        }

        /// <summary>
        ///  This event is invoked if an unexpected exception is thrown.
        /// </summary>
        public event Action<object, Exception> Error;

        /// <summary>
        /// This event is invoked if a package is received.
        /// </summary>
        public event EventHandler<SocketReceiveEventArgs> Receive;

        /// <summary>
        /// Gets the IP end point.
        /// </summary>
        public IPEndPoint EndPoint { get; private set; }

        /// <summary>
        /// Gets the total number of bytes received.
        /// </summary>
        public long TotalBytesReceived
        {
            get
            {
                return bytesReceived;
            }
        }
    }
}
