using System;

namespace Photon.SocketServer.ServerToServer
{
    /// <summary>
    /// Provides data for the <see cref="E:Photon.SocketServer.ServerToServer.TcpClient.OperationResponse"/> event.
    /// </summary>
    public class OperationResponseEventArgs : EventArgs
    {
        /// <summary>
        /// The operaiton response.
        /// </summary>
        private readonly OperationResponse operationResponse;

        /// <summary>
        /// Backing field of <see cref="P:Photon.SocketServer.ServerToServer.OperationResponseEventArgs.SendParameters"/>
        /// </summary>
        private readonly SendParameters sendParameters;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Photon.SocketServer.ServerToServer.OperationResponseEventArgs"/> class.
        /// </summary>
        /// <param name="operationResponse">The received operation response.</param>
        /// <param name="sendParameters">The send parameters the response was received with.</param>
        public OperationResponseEventArgs(OperationResponse operationResponse, SendParameters sendParameters)
        {
            this.operationResponse = operationResponse;
            this.sendParameters = sendParameters;
        }

        /// <summary>
        /// Gets the operation response.
        /// </summary>
        public OperationResponse OperationResponse
        {
            get
            {
                return this.operationResponse;
            }
        }

        /// <summary>
        /// Gets the send parameters the response was received with
        /// </summary>
        public SendParameters SendParameters
        {
            get
            {
                return this.sendParameters;
            }
        }
    }
}
