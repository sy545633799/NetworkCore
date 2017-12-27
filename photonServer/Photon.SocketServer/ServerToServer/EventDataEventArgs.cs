using System;

namespace Photon.SocketServer.ServerToServer
{
    /// <summary>
    ///  Provides data for the <see cref="E:Photon.SocketServer.ServerToServer.TcpClient.Event"/> event.
    /// </summary>
    public class EventDataEventArgs : EventArgs
    {
        /// <summary>
        /// The reveived event data .
        /// </summary>
        private readonly IEventData eventData;

        /// <summary>
        /// Backing field of <see cref="P:Photon.SocketServer.ServerToServer.EventDataEventArgs.SendParameters"/>
        /// </summary>
        private readonly SendParameters sendParameters;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Photon.SocketServer.ServerToServer.EventDataEventArgs"/> class.
        /// </summary>
        /// <param name="eventData">The event data received from the server.</param>
        /// <param name="sendParameters">The send parameters the event was received with.</param>
        public EventDataEventArgs(IEventData eventData, SendParameters sendParameters)
        {
            this.eventData = eventData;
            this.sendParameters = sendParameters;
        }

        /// <summary>
        /// Gets the event data received from the server.
        /// </summary>
        /// <value>The event data.</value>
        public IEventData EventData
        {
            get
            {
                return this.eventData;
            }
        }

        /// <summary>
        /// Gets the send parameters the event was received with
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
