using System;
using System.Net.Sockets;
using System.Threading;
using ExitGames.Logging;
using ExitGames.Threading;

namespace ExitGames.Net.Sockets.Pgm
{
    /// <summary>
    /// A pool of <see cref="T:ExitGames.Net.Sockets.Pgm.PgmSender"/>s.
    /// </summary>
    public sealed class PgmSenderPool : IDisposable
    {
        /// <summary>
        /// The logger.
        /// </summary>
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The multicast address.
        /// </summary>
        private readonly string address;

        /// <summary>
        /// The interface to bind to.
        /// </summary>
        private readonly string bindInterface;

        /// <summary>
        /// The multicast port.
        /// </summary>
        private readonly int port;

        /// <summary>
        /// The pool of <see cref="T:ExitGames.Net.Sockets.Pgm.PgmSender"/> instances.
        /// </summary>
        private readonly BlockingQueue<PgmSender> queue;

        /// <summary>
        /// The <see cref="T:ExitGames.Net.Sockets.Pgm.PgmSender"/> pool size.
        /// </summary>
        private readonly int queueSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ExitGames.Net.Sockets.Pgm.PgmSenderPool"/> class.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="port">The port.</param>
        /// <param name="bindInterface">The bind interface.</param>
        /// <param name="queueSize"> The queue size.</param>
        /// <param name="lockTimeout">The lock timeout.</param>
        public PgmSenderPool(string address, int port, string bindInterface, int queueSize, int lockTimeout)
        {
            if (log.IsInfoEnabled)
            {
                log.InfoFormat("Creating PgmSourcePool: address={0} , port={1} , bindinterface{2} , queueSize={3}", address, port, bindInterface, queueSize);
            }
            this.address = address;
            this.port = port;
            this.bindInterface = bindInterface;
            this.queueSize = queueSize;
            this.queue = new BlockingQueue<PgmSender>(queueSize, lockTimeout);
            this.Initialize();
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="T:ExitGames.Net.Sockets.Pgm.PgmSenderPool"/> class.
        /// </summary>
        ~PgmSenderPool()
        {
            Dispose(false);
        }

        /// <summary>
        /// Sends a byte array asynchronously.
        /// </summary>
        /// <param name="data">The payload.</param>
        public void Send(byte[] data)
        {
            PgmSender item = queue.Dequeue();
            try
            {
                item.Send(data);
            }
            catch (SocketException exception)
            {
                switch (exception.SocketErrorCode)
                {
                    case SocketError.ConnectionReset:
                    case SocketError.NotConnected:
                    case SocketError.Disconnecting:
                        try
                        {
                            item.Connect(this.bindInterface, null, null);
                        }
                        catch (SocketException exception2)
                        {
                            log.Error("Reconnect failed, " + SocketHelper.FormatSocketException(exception2));
                        }
                        catch (ThreadAbortException)
                        {
                            throw;
                        }
                        catch (OutOfMemoryException)
                        {
                            throw;
                        }
                        catch (Exception exception3)
                        {
                            log.Error("Reconnect failed", exception3);
                        }
                        break;
                }
                throw;
            }
            finally
            {
                queue.Enqueue(item);
            }
        }

        /// <summary>
        /// Disposes all used <see cref="T:ExitGames.Net.Sockets.Pgm.PgmSender"/> instances.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///  Disposes all used <see cref="T:ExitGames.Net.Sockets.Pgm.PgmSender"/> instances.
        /// </summary>
        /// <param name="disposing">True if called from <see cref="M:ExitGames.Net.Sockets.Pgm.PgmSenderPool.Dispose"/>.</param>
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                for (int i = 0; i < queue.MaxSize; i++)
                {
                    queue.Dequeue().Dispose();
                }
            }
        }

        /// <summary>
        /// Creates the <see cref="T:ExitGames.Net.Sockets.Pgm.PgmSender"/> instances.
        /// </summary>
        private void Initialize()
        {
            for (int i = 0; i < queue.MaxSize; i++)
            {
                PgmSender sender = new PgmSender(Address, Port);
                int? sendBufferSize = null;
                PgmSendWindowSize? windowSize = null;
                sender.Connect(bindInterface, sendBufferSize, windowSize);
                queue.Enqueue(sender, i);
            }
        }

        /// <summary>
        /// Gets Address.
        /// </summary>
        public string Address
        {
            get
            {
                return address;
            }
        }

        /// <summary>
        /// Gets BindInterface.
        /// </summary>
        public string BindInterface
        {
            get
            {
                return bindInterface;
            }
        }

        /// <summary>
        /// Gets Port.
        /// </summary>
        public int Port
        {
            get
            {
                return port;
            }
        }

        /// <summary>
        /// Gets QueueSize.
        /// </summary>
        public int QueueSize
        {
            get
            {
                return queueSize;
            }
        }
    }
}
