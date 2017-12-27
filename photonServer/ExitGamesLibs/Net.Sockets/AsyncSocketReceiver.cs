using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using ExitGames.Logging;

namespace ExitGames.Net.Sockets
{
    /// <summary>
    /// The <see cref="T:ExitGames.Net.Sockets.AsyncSocketReceiver"/> receives data from a socket using the asynchronous programming model including IO completion ports.
    /// </summary>
    public abstract class AsyncSocketReceiver : IDisposable
    {
        /// <summary>
        /// The receive action.
        /// </summary>
        private readonly EventHandler<SocketAsyncEventArgs> receiveAction;

        /// <summary>
        /// The logger.
        /// </summary>
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The buffer.
        /// </summary>
        private readonly byte[] buffer;

        /// <summary>
        /// The remote endpoint.
        /// </summary>
        private readonly EndPoint remoteEndpoint;

        /// <summary>
        /// The socket.
        /// </summary>
        private readonly Socket socket;

        /// <summary>
        /// The socket event args.
        /// </summary>
        private readonly SocketAsyncEventArgs socketEventArgs;

        /// <summary>
        /// The receive completed callback.
        /// </summary>
        /// <param name="ar">The ar.</param>
        private void OnReceiveCompletedCallback(IAsyncResult ar)
        {
            try
            {
                this.receiveAction.EndInvoke(ar);
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
        /// Initializes a new instance of the <see cref="T:ExitGames.Net.Sockets.AsyncSocketReceiver"/> class.
        /// </summary>
        /// <param name="socket">The socket.</param>
        /// <param name="bufferSize">The buffer size.</param>
        protected AsyncSocketReceiver(Socket socket, int bufferSize)
        {
            this.receiveAction = new EventHandler<SocketAsyncEventArgs>(this.OnReceiveCompleted);
            this.socket = socket;
            this.remoteEndpoint = this.socket.RemoteEndPoint;
            this.buffer = new byte[bufferSize];
            this.socketEventArgs = new SocketAsyncEventArgs();
            this.socketEventArgs.SetBuffer(this.buffer, 0, this.buffer.Length);
            this.socketEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(this.OnReceiveCompleted);
        }

        /// <summary>
        /// Begins an asynchronous request to receive data from the underlying socket.
        /// </summary>
        public void BeginReceive()
        {
            if (!this.socket.AcceptAsync(this.socketEventArgs))
            {
                this.receiveAction.BeginInvoke(this, this.socketEventArgs, new AsyncCallback(this.OnReceiveCompletedCallback), null);
            }
        }

        /// <summary>
        /// Disposes the receiver.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.socketEventArgs.Completed -= new EventHandler<SocketAsyncEventArgs>(this.OnReceiveCompleted);
                this.socketEventArgs.Dispose();
                this.socket.Dispose();
            }
        }

        /// <summary>
        /// Abstract method that is called when the socket has received new data.
        /// </summary>
        /// <param name="sender">The sender is a <see cref="T:System.Net.Sockets.Socket"/> instance.</param>
        /// <param name="e">The <see cref="T:System.Net.Sockets.SocketAsyncEventArgs"/>.</param>
        protected abstract void OnReceiveCompleted(object sender, SocketAsyncEventArgs e);

        /// <summary>
        /// Gets the size, in bytes, of the buffer for incoming data.
        /// </summary>
        /// <value>The size of the buffer.</value>
        public int BufferSize
        {
            get
            {
                return this.buffer.Length;
            }
        }

        /// <summary>
        /// Gets the remote endpoint of the underlying socket.
        /// </summary>
        /// <value>The remote IP and port.</value>
        public EndPoint RemoteEndpoint
        {
            get
            {
                return this.remoteEndpoint;
            }
        }
    }
}
