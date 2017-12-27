using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using ExitGames.Logging;
using ExitGames.Net.Sockets;

namespace ExitGames.Net.Sockets.Pgm
{
    /// <summary>
    /// Provides methods to send data using the Pragmatic General Multicast (PGM) protocol.
    /// </summary>
    [DebuggerDisplay("PgmSender - [{EndPoint}]")]
    public sealed class PgmSender : IDisposable, ISocketSender
    {
        /// <summary>
        /// The default rate kbits per sec.
        /// </summary>
        /// <value>100 Mbit</value>
        [CLSCompliant(false)]
        public static readonly uint DefaultRateKbitsPerSec = 0x19999;

        /// <summary>
        /// The default window size in bytes.
        /// </summary>
        /// <value >5 seconds</value>
        [CLSCompliant(false)]
        public static readonly uint DefaultWindowSizeInBytes = (((DefaultRateKbitsPerSec * 0x400) / 8) * 5);

        /// <summary>
        /// The default window size in m secs.
        /// </summary>
        [CLSCompliant(false)]
        public static readonly uint DefaultWindowSizeInMSecs = 0;

        /// <summary>
        /// Used to log messages to the logging framework.
        /// </summary>
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Object used for synchronization.
        /// </summary>
        private static readonly object syncRoot = new object();

        /// <summary>
        /// The multicast address.
        /// </summary>
        private readonly IPEndPoint endPoint;

        /// <summary>
        /// The number of bytes sent by this instance.
        /// </summary>
        private long bytesSent;

        /// <summary>
        /// The used <see cref="T:ExitGames.Net.Sockets.Pgm.PgmSocket"/>.
        /// </summary>
        private PgmSocket socket;

        /// <summary>
        /// Initializes static members of the <see cref="T:ExitGames.Net.Sockets.Pgm.PgmSender"/> class.
        /// </summary>
        /// <param name="endPoint">The end point.</param>
        public PgmSender(IPEndPoint endPoint)
        {
            this.endPoint = endPoint;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ExitGames.Net.Sockets.Pgm.PgmSender"/> class.
        /// </summary>
        /// <param name="address">The ip address to which the underling socket will be connected.</param>
        /// <param name="port">The port to which the underling socket will be connected.</param>
        public PgmSender(IPAddress address, int port)
            : this(new IPEndPoint(address, port))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ExitGames.Net.Sockets.Pgm.PgmSender"/> class.
        /// </summary>
        /// <param name="address">The ip address to which the underling socket will be connected.</param>
        /// <param name="port">The port to which the underling socket will be connected.</param>
        public PgmSender(string address, int port)
            : this(IPAddress.Parse(address), port)
        {
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="T:ExitGames.Net.Sockets.Pgm.PgmSender"/> class.
        /// </summary>
        ~PgmSender()
        {
            Dispose(false);
        }

        /// <summary>
        /// Closes the <see cref="T:System.Net.Sockets.Socket"/>.
        /// </summary>
        public void Close()
        {
            lock (syncRoot)
            {
                if (socket != null)
                {
                    socket.Close();
                    socket = null;
                }
            }
        }

        /// <summary>
        /// Connects the <see cref="T:ExitGames.Net.Sockets.Pgm.PgmSender"/> socket.
        /// </summary>
        /// <exception cref="T:System.Net.Sockets.SocketException">
        ///  Exception thrown by the underlying <see cref="T:System.Net.Sockets.Socket"/>.
        /// </exception>
        public void Connect()
        {
            Connect(null, null, null);
        }

        /// <summary>
        /// Connects the <see cref="T:ExitGames.Net.Sockets.Pgm.PgmSender"/> socket.
        /// </summary>
        /// <param name="sendInterface">The send Interface.</param>
        /// <exception cref="T:System.Net.Sockets.SocketException">
        /// Exception thrown by the underlying <see cref="T:System.Net.Sockets.Socket"/>.
        ///</exception>
        public void Connect(string sendInterface)
        {
            Connect(sendInterface, null, null);
        }

        /// <summary>
        /// Connects the <see cref="T:ExitGames.Net.Sockets.Pgm.PgmSender"/> socket.
        ///  Any pre-existing connection is closed.
        /// </summary>
        /// <param name="sendInterface">The send Interface.</param>
        /// <param name="sendBufferSize">The send Buffer Size.</param>
        /// <param name="windowSize">The window Size.</param>
        /// <exception cref="T:System.Net.Sockets.SocketException">
        ///   Exception thrown by the underlying <see cref="T:System.Net.Sockets.Socket"/>.
        ///  </exception>
        [CLSCompliant(false)]
        public void Connect(string sendInterface, int? sendBufferSize, PgmSendWindowSize? windowSize)
        {
            lock (syncRoot)
            {
                if (socket != null)
                {
                    socket.Close();
                    socket = null;
                }
                socket = new PgmSocket();
                int? nullable = sendBufferSize;
                socket.SendBufferSize = nullable.HasValue ? nullable.GetValueOrDefault() : 0x100000;
                if (!string.IsNullOrEmpty(sendInterface))
                {
                    IPAddress interfaceIpAddress = IPAddress.Parse(sendInterface);
                    this.socket.SetSendInterface(interfaceIpAddress);
                }
                socket.Bind(new IPEndPoint(IPAddress.Any, 50001));
                if (!windowSize.HasValue)
                {
                    windowSize = new PgmSendWindowSize(DefaultRateKbitsPerSec, DefaultWindowSizeInMSecs, DefaultWindowSizeInBytes);
                }
                socket.SetSendWindowSize(windowSize.Value);
                socket.Connect(EndPoint);
            }
        }

        /// <summary>
        /// Releases the resources used by the <see cref="T:ExitGames.Net.Sockets.Pgm.PgmSender"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Sends data to a connected Socket.
        /// </summary>
        /// <param name="data">An array of type Byte that contains the data to be sent. </param>
        public void Send(byte[] data)
        {
            int num;

            lock (syncRoot)
            {
                if (socket == null)
                {
                    throw new InvalidOperationException("PgmSender not started");
                }
                num = socket.Send(data, SocketFlags.None);
            }
            Interlocked.Add(ref bytesSent, num);
        }

        /// <summary>
        /// Sends data to a connected Socket.
        /// </summary>
        /// <param name="data">An array of type Byte that contains the data to be sent. </param>
        /// <param name="offset">The position in the data buffer at which to begin sending data.</param>
        /// <param name="length">The number of bytes to send.</param>
        public void Send(byte[] data, int offset, int length)
        {
            int num;
            lock (syncRoot)
            {
                if (socket == null)
                {
                    throw new InvalidOperationException("PgmSender not started");
                }
                num = socket.Send(data, offset, length, SocketFlags.None);
            }
            Interlocked.Add(ref bytesSent, num);
        }

        /// <summary>
        /// Sends a set of buffers in the list to a connected Socket.
        /// </summary>
        /// <param name="data"> A list of ArraySegments of type Byte that contains the data to be sent.</param>
        public void Send(IList<ArraySegment<byte>> data)
        {
            int num;
            lock (syncRoot)
            {
                if (socket == null)
                {
                    throw new InvalidOperationException("PgmSender not started");
                }
                num = socket.Send(data);
            }
            Interlocked.Add(ref bytesSent, num);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="T:ExitGames.Net.Sockets.Pgm.PgmSender"/>
        /// and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">Indicates if a dispose is in progress.</param>
        /// <remarks>
        ///   Dispose(bool disposing) executes in two distinct scenarios.
        ///   If disposing equals true, the method has been called directly
        ///   or indirectly by a user's code. Managed and unmanaged resources
        ///   can be disposed.
        ///   If disposing equals false, the method has been called by the
        ///   runtime from inside the finalizer and you should not reference
        ///    other objects. Only unmanaged resources can be disposed.
        ///  </remarks>
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    Close();
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
                    if (log.IsWarnEnabled)
                    {
                        log.Warn("Failed to close socket", exception);
                    }
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:ExitGames.Net.Sockets.ISocketSender"/> is connected 
        ///   to a remote host as of the last Send operation.
        /// </summary>
        public bool Connected
        {
            get
            {
                return socket != null && socket.Connected;
            }
        }

        /// <summary>
        /// Gets the multicast end point.
        /// </summary>
        /// <value>The multicast end point.</value>
        public IPEndPoint EndPoint
        {
            get
            {
                return endPoint;
            }
        }

        /// <summary>
        /// Gets or sets the total number of sent bytes.
        /// </summary>
        /// <value>The total number of sent bytes.</value>
        public long TotalBytesSent
        {
            get
            {
                return Interlocked.Read(ref bytesSent);
            }
            set
            {
                Interlocked.Exchange(ref bytesSent, value);
            }
        }
    }
}
