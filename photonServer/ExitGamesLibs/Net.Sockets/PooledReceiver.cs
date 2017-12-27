using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using ExitGames.Concurrency.Fibers;
using ExitGames.Logging;
using ExitGames.Net.Sockets;

namespace ExitGames.Net.Sockets
{
    /// <summary>
    ///  This <see cref="T:ExitGames.Net.Sockets.ISocketReceiver"/> wrapper dispatches received data async with the .NET thread pool.
    /// </summary>
    public sealed class PooledReceiver : IDisposable, ISocketReceiver
    {
        /// <summary>
        /// The used fiber.
        /// </summary>
        public readonly PoolFiber Fiber = new PoolFiber();

        /// <summary>
        /// <see cref="T:ExitGames.Logging.ILogger"/> instance used to log messages to the logging framework.
        /// </summary>
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        ///  The receive callback.
        /// </summary>
        private readonly Action<ReceiveBuffer> receiveAction;

        /// <summary>
        /// The underlying receiver.
        /// </summary>
        private readonly ISocketReceiver receiver;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ExitGames.Net.Sockets.PooledReceiver"/> class.
        /// </summary>
        /// <param name="receiver">The underlying receiver.</param>
        /// <param name="batchedSender">Indicates whether to expected message batches from the <see
        /// cref="T:ExitGames.Net.Sockets.PooledSender"/>.
        /// </param>
        public PooledReceiver(ISocketReceiver receiver, bool batchedSender)
        {
            Fiber.Start();
            if (batchedSender)
            {
                receiveAction = new Action<ReceiveBuffer>(ProcessBufferBatched);
            }
            else
            {
                receiveAction = new Action<ReceiveBuffer>(ProcessBuffer);
            }
            this.receiver = receiver;
            this.receiver.Receive += new EventHandler<SocketReceiveEventArgs>(Receiver_OnReceive);
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="T:ExitGames.Net.Sockets.PooledReceiver"/> class.
        /// </summary>
        ~PooledReceiver()
        {
            Dispose(false);
        }

        /// <summary>
        /// Resets message and package count statistics.
        /// </summary>
        public void ResetStatistics()
        {
            ReceivedPackages = 0;
            ReceivedMessages = 0;
        }

        /// <summary>
        /// Disposes the instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the instance.
        /// </summary>
        /// <param name="disposing">The disposing.</param>
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (log.IsDebugEnabled)
                {
                    log.Debug("Disposing BufferedReceiver.");
                }
                receiver.Receive -= new EventHandler<SocketReceiveEventArgs>(Receiver_OnReceive);
                Fiber.Dispose();
            }
        }

        /// <summary>
        /// Invokes the <see cref="E:ExitGames.Net.Sockets.PooledReceiver.Receive"/> event.
        /// </summary>
        /// <param name="remoteEndPoint">The remote end point.</param>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        private void OnReceive(EndPoint remoteEndPoint, byte[] buffer, int offset, int length)
        {
            EventHandler<SocketReceiveEventArgs> handler = Receive;
            if (handler != null)
            {
                SocketReceiveEventArgs e = new SocketReceiveEventArgs(remoteEndPoint, buffer, offset, length);
                handler(this, e);
            }
        }

        /// <summary>
        /// Updates statistics and calls <see 
        /// cref="M:ExitGames.Net.Sockets.PooledReceiver.OnReceive(System.Net.EndPoint,System.Byte[],System.Int32,System.Int32)"/>.
        /// </summary>
        /// <param name="buffer">The received buffer.</param>
        private void ProcessBuffer(ReceiveBuffer buffer)
        {
            try
            {
                ReceivedMessages = ReceivedMessages + 1;
                ReceivedPackages = ReceivedPackages + 1;
                OnReceive(buffer.EndPoint, buffer.Data, 0, buffer.Data.Length);
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
                log.Error(exception);
            }
        }
        /// <summary>
        /// Updates statistics and calls <see 
        /// cref="M:ExitGames.Net.Sockets.PooledReceiver.OnReceive(System.Net.EndPoint,System.Byte[],System.Int32,System.Int32)"/> for each message in the batch.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        private void ProcessBufferBatched(ReceiveBuffer buffer)
        {
            try
            {
                BatchMessage batchMessage = BatchMessage.FromBinary(buffer.Data);
                ReceivedPackages = ReceivedPackages + 1;
                ReceivedMessages = ReceivedMessages + batchMessage.MessageCount;
                for (int i = 0; i < batchMessage.MessageCount; i++)
                {
                    ArraySegment<byte> segment = batchMessage[i];
                    OnReceive(buffer.EndPoint, segment.Array, segment.Offset, segment.Count);
                }
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
                log.Error(exception);
            }
        }

        /// <summary>
        /// Receives the data from the underlying <see
        /// cref="T:ExitGames.Net.Sockets.ISocketReceiver"/> and queues them to the <see 
        /// cref="T:ExitGames.Concurrency.Fibers.PoolFiber"/>.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void Receiver_OnReceive(object sender, SocketReceiveEventArgs e)
        {
            //[CompilerGenerated]
            //private sealed class #K
            //{
            //    // Fields
            //    public PooledReceiver.#D #a;
            //    public PooledReceiver #b;

            //    // Methods
            //    public void #zc()
            //    {
            //        this.#b.#b(this.#a);
            //    }
            //}

            //#K #k = new #K {
            //    #b = this,
            //    #a = new #D(args1)
            //};
            //this.Fiber.Enqueue(new Action(#k.#zc));

            try
            {
                Fiber.Enqueue(() => receiveAction(new ReceiveBuffer(e)));
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
                if (log.IsErrorEnabled)
                {
                    log.Error(exception);
                }
            }
        }

        /// <summary>
        /// This event is invoked when new data is received.
        /// </summary>
        public event EventHandler<SocketReceiveEventArgs> Receive;

        /// <summary>
        /// Gets the remote end point.
        /// </summary>
        /// <value>Returns the underlying <see cref="P:ExitGames.Net.Sockets.ISocketReceiver.EndPoint"/>.</value>
        public IPEndPoint EndPoint
        {
            get
            {
                return receiver.EndPoint;
            }
        }

        /// <summary>
        /// Gets the number of messages received.
        /// </summary>
        /// <value>The number of messages received.</value>
        public int ReceivedMessages { get; private set; }

        /// <summary>
        /// Gets the number of packages received.
        /// </summary>
        /// <value>The number of packages received.</value>
        public int ReceivedPackages { get; private set; }

        /// <summary>
        /// Gets the total number of bytes received.
        /// </summary>
        /// <value>Returns the underlying <see cref="P:ExitGames.Net.Sockets.ISocketReceiver.TotalBytesReceived"/>.</value>
        public long TotalBytesReceived
        {
            get
            {
                return receiver.TotalBytesReceived;
            }
        }

        /// <summary>
        /// Private struct for buffering incoming data.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct ReceiveBuffer
        {
            /// <summary>
            ///  The data.
            /// </summary>
            public readonly byte[] Data;

            /// <summary>
            /// The remote end point.
            /// </summary>
            public readonly EndPoint EndPoint;

            /// <summary>
            /// Initializes a new instance of the <see
            /// cref="T:ExitGames.Net.Sockets.PooledReceiver.ReceiveBuffer"/> struct.
            /// </summary>
            /// <param name="e">The e.</param>
            public ReceiveBuffer(SocketReceiveEventArgs e)
            {
                this.EndPoint = e.RemoteEndPoint;
                this.Data = new byte[e.BytesReceived];
            
                //.(e.Buffer, e.Offset, this.Data, 0, e.BytesReceived);
            }
        }
    }
}
