using System;
using System.Collections.Generic;
using System.Net;

namespace ExitGames.Net.Sockets.Pgm
{
    /// <summary>
    /// This <see cref="T:ExitGames.Net.Sockets.ISocketSender"/> wraps a <see 
    /// cref="T:ExitGames.Net.Sockets.PooledSender"/> and uses a <see 
    /// cref="T:ExitGames.Net.Sockets.Pgm.PgmSender"/>.
    /// </summary>
    public sealed class PgmSocketSender : IDisposable, ISocketSender
    {
        /// <summary>
        /// The used <see cref="T:ExitGames.Net.Sockets.PooledSender"/>.
        /// </summary>
        public readonly PooledSender PooledSender;

        /// <summary>
        /// The used  <see cref="T:ExitGames.Net.Sockets.Pgm.PgmSender"/>.
        /// </summary>
        public readonly PgmSender SocketSender;

        /// <summary>
        /// Initializes a new instance of the <see 
        /// cref="T:ExitGames.Net.Sockets.Pgm.PgmSocketSender"/> class.
        /// </summary>
        /// <param name="ip">The ip.</param>
        /// <param name="port">The port.</param>
        public PgmSocketSender(string ip, int port)
            : this(ip, port, true, PooledSender.DefaultBatchSize, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see
        /// cref="T:ExitGames.Net.Sockets.Pgm.PgmSocketSender"/> class.
        /// </summary>
        /// <param name="ip">The ip.</param>
        /// <param name="port">The port.</param>
        /// <param name="bindInterface">The bind interface.</param>
        public PgmSocketSender(string ip, int port, string bindInterface)
            : this(ip, port, true, PooledSender.DefaultBatchSize, null, bindInterface)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see 
        /// cref="T:ExitGames.Net.Sockets.Pgm.PgmSocketSender"/> class.
        /// </summary>
        /// <param name="ip">The ip.</param>
        /// <param name="port"> The port.</param>
        /// <param name="sendBatched">The send batched.</param>
        /// <param name="batchSize">The batch size.</param>
        /// <param name="sendWindowSize">The send window size.</param>
        /// <param name="bindInterface">The bind interface.</param>
        [CLSCompliant(false)]
        public PgmSocketSender(string ip, int port, bool sendBatched, int batchSize, PgmSendWindowSize? sendWindowSize, string bindInterface)
        {
            SocketSender = new PgmSender(ip, port);
            if (!string.IsNullOrEmpty(bindInterface))
            {
                IPAddress address;
                if (IPAddress.TryParse(bindInterface, out address))
                {
                    bindInterface = address.ToString();
                }
                else
                {
                    IPHostEntry entry = Dns.GetHostEntry(bindInterface);
                    bindInterface = entry.AddressList[0].ToString();
                }
            }
            PooledSender = new PooledSender(SocketSender, sendBatched, batchSize);
            SocketSender.Connect(bindInterface, null, sendWindowSize);
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="T:ExitGames.Net.Sockets.Pgm.PgmSocketSender"/> class.
        /// </summary>
        ~PgmSocketSender()
        {
            Dispose(false);
        }

        /// <summary>
        ///  Disposes this instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.
        /// </param>
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                PooledSender.Dispose();
                SocketSender.Dispose();
            }
        }

        /// <summary>
        /// Sends some data.
        /// </summary>
        /// <param name="data">The data.</param>
        public void Send(IList<ArraySegment<byte>> data)
        {
            PooledSender.Send(data);
        }

        /// <summary>
        /// Sends some data.
        /// </summary>
        /// <param name="data">The data.</param>
        public void Send(byte[] data)
        {
            PooledSender.Send(data);
        }

        /// <summary>
        ///  Sends some data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        public void Send(byte[] data, int offset, int length)
        {
            PooledSender.Send(data, offset, length);
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:ExitGames.Net.Sockets.ISocketSender"/> is connected 
        ///    to a remote host as of the last Send operation.
        /// </summary>
        public bool Connected
        {
            get
            {
                return SocketSender.Connected;
            }
        }

        /// <summary>
        /// Gets the multicast end point.
        /// </summary>
        /// <value> The multicast end point.</value>
        public IPEndPoint EndPoint
        {
            get
            {
                return SocketSender.EndPoint;
            }
        }

        /// <summary>
        /// Gets or sets the total bytes sent.
        /// </summary>
        /// <value> The total bytes sent.</value>
        public long TotalBytesSent
        {
            get
            {
                return SocketSender.TotalBytesSent;
            }
            set
            {
                SocketSender.TotalBytesSent = value;
            }
        }
    }
}
