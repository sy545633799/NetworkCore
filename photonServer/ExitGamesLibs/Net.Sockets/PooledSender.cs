using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using ExitGames.Concurrency.Core;
using ExitGames.Concurrency.Fibers;
using ExitGames.Logging;

namespace ExitGames.Net.Sockets
{
    /// <summary>
    ///  This <see cref="T:ExitGames.Net.Sockets.ISocketSender"/> wrapper is used to send the data async using a the .NET thread pool.
    ///  An optional settings allows the <see cref="T:ExitGames.Net.Sockets.PooledSender"/> to send multiple messages in a batch.
    /// </summary>
    public sealed class PooledSender : IDisposable, ISocketSender
    {
        /// <summary>
        /// The default batch size. 
        /// </summary>
        /// <value>32 KB</value>
        public static readonly int DefaultBatchSize = 0x8000;
        /// <summary>
        /// <see cref="T:ExitGames.Logging.ILogger"/> instance used to log messages to the logging framework.
        /// </summary>
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The batch message.
        /// </summary>
        private readonly BatchMessage batchMessage;

        /// <summary>
        /// The enqueue action.
        /// </summary>
        private readonly Action<ArraySegment<byte>> enqueueAction;

        /// <summary>
        ///<see cref="T:ExitGames.Concurrency.Fibers.PoolFiber"/> used to send data.
        /// </summary>
        private readonly PoolFiber fiber;

        /// <summary>
        /// The package size.
        /// </summary>
        private readonly int packageSize;

        /// <summary>
        /// <see cref="T:ExitGames.Net.Sockets.Pgm.PgmSender"/> used to send data.
        /// </summary>
        private readonly ISocketSender sender;

        /// <summary>
        /// A list of <see cref="T:System.ArraySegment`1"/>s.
        /// </summary>
        private Action<IList<ArraySegment<byte>>> sendArraySegmentListAction;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ExitGames.Net.Sockets.PooledSender"/> class.
        /// </summary>
        /// <param name="sender">The <see cref="T:ExitGames.Net.Sockets.ISocketSender"/> that is used to send the data.</param>
        public PooledSender(ISocketSender sender)
            : this(sender, true, DefaultBatchSize)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ExitGames.Net.Sockets.PooledSender"/> class.
        /// </summary>
        /// <param name="sender">The <see cref="T:ExitGames.Net.Sockets.ISocketSender"/> that is used to send the data.</param>
        /// <param name="sendBatched">Indicates whether multiple data should be send batched.</param>
        /// <param name="packageSize">The size for batch packages.</param>
        public PooledSender(ISocketSender sender, bool sendBatched, int packageSize)
        {
            //Action<IList<ArraySegment<byte>>> action = null;
            //Action<ArraySegment<byte>> action2 = null;
            //Action<ArraySegment<byte>> action3 = null;
            this.batchMessage = new BatchMessage();
            this.sender = sender;
            this.packageSize = packageSize;
            sendArraySegmentListAction = new Action<IList<ArraySegment<byte>>>(sender.Send);
            if (sendBatched)
            {
                enqueueAction = new Action<ArraySegment<byte>>(batchMessage.AddMessage);
                fiber = new PoolFiber(new SendBatchedExecutor(this));
            }
            else
            {
                // action3 = delegate(ArraySegment<byte> s) { SendArraySegmentList(new ArraySegment<byte>[] { s }); };
                enqueueAction = s => SendArraySegmentList(new ArraySegment<byte>[] { s });
                fiber = new PoolFiber();
            }
            fiber.Start();
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="T:ExitGames.Net.Sockets.PooledSender"/> class.
        /// </summary>
        ~PooledSender()
        {
            Dispose(false);
        }

        /// <summary>
        /// Disposes the <see cref="T:ExitGames.Net.Sockets.PooledSender"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Sends a byte array to the socket.
        /// </summary>
        /// <param name="data">The data.</param>
        public void Send(byte[] data)
        {
            Send(data, 0, data.Length);
        }

        /// <summary>
        /// Sends a byte array to the socket.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        public void Send(byte[] data, int offset, int length)
        {
            fiber.Enqueue(() => enqueueAction(new ArraySegment<byte>(data, offset, length)));
        }

        /// <summary>
        /// Sends a list of <see cref="T:System.ArraySegment`1"/> of type byte to the socket.
        /// </summary>
        /// <param name="data">The data.</param>
        public void Send(IList<ArraySegment<byte>> data)
        {
            IEnumerator<ArraySegment<byte>> enumerator = data.GetEnumerator();
            foreach (ArraySegment<byte> arraySegment in data)
            {
                fiber.Enqueue(() => enqueueAction(arraySegment));
            }
        }

        /// <summary>
        /// Does nothing. Used to disable the sending immediately.
        /// </summary>
        /// <param name="a">The data.</param>
        private static void DontSendArraySegmentList(IList<ArraySegment<byte>> a)
        {
        }

        /// <summary>
        /// Disposes the instance.
        /// </summary>
        /// <param name="disposing">The disposing.</param>
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                sendArraySegmentListAction = Interlocked.Exchange<Action<IList<ArraySegment<byte>>>>(ref sendArraySegmentListAction, new Action<IList<ArraySegment<byte>>>(PooledSender.DontSendArraySegmentList));
                fiber.Dispose();
            }
        }

        /// <summary>
        /// Invokes <see cref="T:ExitGames.Net.Sockets.PooledSender.SendErrorEventArgs"/>.
        /// </summary>
        /// <param name="e">The exception.</param>
        /// <param name="payload">The payload.</param>
        private void OnSendError(Exception e, IList<ArraySegment<byte>> payload)
        {
            EventHandler<SendErrorEventArgs> handler = SendError;
            if (handler != null)
            {
                handler(this, new SendErrorEventArgs(e, payload));
            }
            else
            {
                log.Error(e);
            }
        }

        /// <summary>
        /// Sends an array segment list.
        /// </summary>
        /// <param name="a">The a.</param>
        private void SendArraySegmentList(IList<ArraySegment<byte>> a)
        {
            try
            {
                sendArraySegmentListAction(a);
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
                OnSendError(exception, a);
            }
        }

        /// <summary>
        /// The send batch message.
        /// </summary>
        private void SendBatchMessage()
        {
            List<ArraySegment<byte>> list;
            try
            {
                list = batchMessage.ToBinary();
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
                return;
            }
            SendArraySegmentList(list);
            batchMessage.Clear();
        }

        /// <summary>
        /// This event is invoked if an unexpected error occurs.
        /// </summary>
        public event EventHandler<SendErrorEventArgs> SendError;

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:ExitGames.Net.Sockets.ISocketSender"/> is connected 
        ///  to a remote host as of the last Send operation.
        /// </summary>
        public bool Connected
        {
            get
            {
                return sender.Connected;
            }
        }

        /// <summary>
        /// Gets the remote end point.
        /// </summary>
        /// <value>Returns the underlying <see cref="P:ExitGames.Net.Sockets.ISocketSender.EndPoint"/>.</value>
        public IPEndPoint EndPoint
        {
            get
            {
                return sender.EndPoint;
            }
        }

        /// <summary>
        ///  Gets the internally used fiber.
        /// </summary>
        public PoolFiber Fiber
        {
            get
            {
                return fiber;
            }
        }

        /// <summary>
        /// Gets the total number of bytes sent.
        /// </summary>
        /// <value>Returns the underlying <see cref="P:ExitGames.Net.Sockets.ISocketSender.TotalBytesSent"/>.</value>
        public long TotalBytesSent
        {
            get
            {
                return sender.TotalBytesSent;
            }
        }

        /// <summary>
        /// The event args for event <see cref="E:ExitGames.Net.Sockets.PooledSender.SendError"/>.
        /// </summary>
        public sealed class SendErrorEventArgs : EventArgs
        {
            /// <summary>
            /// The exception.
            /// </summary>
            private readonly Exception exception;

            /// <summary>
            /// The payload.
            /// </summary>
            private readonly IList<ArraySegment<byte>> payload;

            /// <summary>
            /// Initializes a new instance of the <see cref="T:ExitGames.Net.Sockets.PooledSender.SendErrorEventArgs"/> class.
            /// </summary>
            /// <param name="exception">The exception.</param>
            /// <param name="payload"> The payload.</param>
            internal SendErrorEventArgs(Exception exception, IList<ArraySegment<byte>> payload)
            {
                this.exception = exception;
                this.payload = payload;
            }

            /// <summary>
            /// Gets the unhandled exception.
            /// </summary>
            /// <value>  The excepton.</value>
            public Exception Exception
            {
                get
                {
                    return this.exception;
                }
            }

            /// <summary>
            /// Gets the payload that could not be sent due to the exception.
            /// </summary>
            /// <value>The payload.</value>
            public IList<ArraySegment<byte>> Payload
            {
                get
                {
                    return this.payload;
                }
            }
        }

        /// <summary>
        /// The send batched executor.
        /// </summary>
        private class SendBatchedExecutor : IExecutor
        {
            /// <summary>
            /// The sender.
            /// </summary>
            private readonly PooledSender sender;

            /// <summary>
            /// Initializes a new instance of the <see cref="T:ExitGames.Net.Sockets.PooledSender.SendBatchedExecutor"/> class.
            /// </summary>
            /// <param name="sender">The sender.</param>
            public SendBatchedExecutor(PooledSender sender)
            {
                this.sender = sender;
            }

            /// <summary>
            /// Executes one action and sends the batch if full.
            /// </summary>
            /// <param name="action">The action.</param>
            public void Execute(Action action)
            {
                try
                {
                    action();
                    if (this.sender.batchMessage.Size >= this.sender.packageSize)
                    {
                        this.sender.SendBatchMessage();
                    }
                    if (this.sender.batchMessage.MessageCount > 0)
                    {
                        this.sender.SendBatchMessage();
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
                    PooledSender.log.Error(exception);
                }
            }

            /// <summary>
            /// Executes the actions and send the batch if full.
            /// </summary>
            /// <param name="actions">The actions</param>
            public void Execute(List<Action> actions)
            {
                foreach (Action action in actions)
                {
                    action();
                    if (this.sender.batchMessage.Size >= this.sender.packageSize)
                    {
                        this.sender.SendBatchMessage();
                    }
                }
                if (this.sender.batchMessage.MessageCount > 0)
                {
                    this.sender.SendBatchMessage();
                }
            }
        }
    }
}
