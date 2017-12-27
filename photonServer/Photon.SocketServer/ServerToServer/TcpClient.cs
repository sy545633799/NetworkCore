using System;
using System.Net.Sockets;

namespace Photon.SocketServer.ServerToServer
{
    /// <summary>
    /// Provides methods to communicate with other photon server applications.
    /// </summary>
    public class TcpClient : TcpClientBase
    {
        /// <summary>
        /// Invoked if an async operation completed with a <see cref="T:System.Net.Sockets.SocketError"/>.
        /// </summary>
        public event EventHandler<SocketErrorEventArgs> AsyncSocketError;

        /// <summary>
        /// Invoked when the client succussfully connected to the remote host.
        /// </summary>
        public event EventHandler ConnectCompleted;

        /// <summary>
        /// Invoked when an error occures during a connection attempt.
        /// </summary>
        public event EventHandler<SocketErrorEventArgs> ConnectError;

        /// <summary>
        ///  Invoked when the client has been diconnected.
        /// </summary>
        public event EventHandler<SocketErrorEventArgs> Disconnected;

        /// <summary>
        ///   Occurs when an event has been received from the remote host.
        /// </summary>
        public event EventHandler<EventDataEventArgs> Event;

        /// <summary>
        /// Occurs when an inittlaize encryption response has been received from the remote host.
        /// </summary>
        public event EventHandler<InitializeEncryptionEventArgs> InitializeEncryptionCompleted;

        /// <summary>
        /// Occurs when an operation response hav been received from the remote host.
        /// </summary>
        public event EventHandler<OperationResponseEventArgs> OperationResponse;

        /// <summary>
        ///  Occurs when a ping response has been received from the remote host.
        /// </summary>
        public event EventHandler<PingResponseEventArgs> PingResponse;

        /// <summary>
        ///  Initializes a new instance of the <see cref="T:Photon.SocketServer.ServerToServer.TcpClient"/> class.
        /// </summary>
        public TcpClient()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Photon.SocketServer.ServerToServer.TcpClient"/> class.
        /// </summary>
        /// <param name="protocol"> The <see cref="T:Photon.SocketServer.IRpcProtocol"/> to use for operation and event serialization.</param>
        public TcpClient(IRpcProtocol protocol)
            : base(protocol, null)
        {
        }

        /// <summary>
        ///  Initializes a new instance of the <see cref="T:Photon.SocketServer.ServerToServer.TcpClient"/> class.
        /// </summary>
        /// <param name="clientVersion"> The client version.</param>
        public TcpClient(Version clientVersion)
            : base(clientVersion)
        {
        }

        /// <summary>
        ///  Initializes a new instance of the <see cref="T:Photon.SocketServer.ServerToServer.TcpClient"/> class.
        /// </summary>
        /// <param name="protocol"> The <see cref="T:Photon.SocketServer.IRpcProtocol"/> to use for operation and event serialization.</param>
        /// <param name="clientVersion"> The client version.</param>
        public TcpClient(IRpcProtocol protocol, Version clientVersion)
            : base(protocol, clientVersion)
        {
        }

        /// <summary>
        ///    Invokes the <see cref="E:Photon.SocketServer.ServerToServer.TcpClient.AsyncSocketError"/> event.
        /// </summary>
        /// <param name="socketError"> The socket error.</param>
        protected override void OnAsyncSocketError(SocketError socketError)
        {
            EventHandler<SocketErrorEventArgs> asyncSocketError = this.AsyncSocketError;
            if (asyncSocketError != null)
            {
                asyncSocketError(this, new SocketErrorEventArgs(socketError));
            }
        }

        /// <summary>
        /// Raises the <see cref="E:Photon.SocketServer.ServerToServer.TcpClient.ConnectCompleted"/> event.
        /// </summary>
        protected override void OnConnectCompleted()
        {
            EventHandler connectCompleted = this.ConnectCompleted;
            if (connectCompleted != null)
            {
                connectCompleted(this, EventArgs.Empty);
            }
        }

        /// <summary>
        ///  Raises the <see cref="E:Photon.SocketServer.ServerToServer.TcpClient.ConnectError"/> event.
        /// </summary>
        /// <param name="error">The socket error which occured during teh connection attempt.</param>
        protected override void OnConnectError(SocketError error)
        {
            EventHandler<SocketErrorEventArgs> connectError = this.ConnectError;
            if (connectError != null)
            {
                connectError(this, new SocketErrorEventArgs(error));
            }
        }

        /// <summary>
        ///  Raises the <see cref="E:Photon.SocketServer.ServerToServer.TcpClient.Disconnected"/> event.
        /// </summary>
        /// <param name="socketError"> The socket error code.</param>
        protected override void OnDisconnect(SocketError socketError)
        {
            EventHandler<SocketErrorEventArgs> disconnected = this.Disconnected;
            if (disconnected != null)
            {
                disconnected(this, new SocketErrorEventArgs(socketError));
            }
        }

        /// <summary>
        ///   Raises the <see cref="E:Photon.SocketServer.ServerToServer.TcpClient.Event"/> event.
        /// </summary>
        /// <param name="eventData">The event data.</param>
        /// <param name="sendParameters">The send parameters the response was received with.</param>
        protected override void OnEvent(IEventData eventData, SendParameters sendParameters)
        {
            EventHandler<EventDataEventArgs> handler = this.Event;
            if (handler != null)
            {
                handler(this, new EventDataEventArgs(eventData, sendParameters));
            }
        }

        /// <summary>
        ///  Raises the <see cref="E:Photon.SocketServer.ServerToServer.TcpClient.InitializeEncryptionCompleted"/> event.
        /// </summary>
        /// <param name="resultCode">The result code received from the remote host.</param>
        /// <param name="debugMessage">The debug message received from the remote host.</param>
        protected override void OnInitializeEcryptionCompleted(short resultCode, string debugMessage)
        {
            EventHandler<InitializeEncryptionEventArgs> initializeEncryptionCompleted = this.InitializeEncryptionCompleted;
            if (initializeEncryptionCompleted != null)
            {
                initializeEncryptionCompleted(this, new InitializeEncryptionEventArgs(resultCode, debugMessage));
            }
        }

        /// <summary>
        ///  Raises the <see cref="E:Photon.SocketServer.ServerToServer.TcpClient.OperationResponse"/> event.
        /// </summary>
        /// <param name="operationResponse">The operation response.</param>
        /// <param name="sendParameters">The send parameters the response was received with.</param>
        protected override void OnOperationResponse(OperationResponse operationResponse, SendParameters sendParameters)
        {
            EventHandler<OperationResponseEventArgs> handler = this.OperationResponse;
            if (handler != null)
            {
                handler(this, new OperationResponseEventArgs(operationResponse, sendParameters));
            }
        }

        /// <summary>
        ///  Raises the <see cref="E:Photon.SocketServer.ServerToServer.TcpClient.PingResponse"/> event.
        /// </summary>
        /// <param name="pingResponse">The ping response.</param>
        protected override void OnPingResponse(PingResponse pingResponse)
        {
            EventHandler<PingResponseEventArgs> handler = this.PingResponse;
            if (handler != null)
            {
                handler(this, new PingResponseEventArgs(pingResponse));
            }
        }
    }
}
