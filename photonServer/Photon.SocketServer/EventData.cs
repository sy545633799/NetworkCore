using System;
using System.Collections.Generic;
using Photon.SocketServer.Rpc;
using Photon.SocketServer.Rpc.Reflection;

namespace Photon.SocketServer
{
    /// <summary>
    /// This class contains data that is sent to clients with <see cref="M:Photon.SocketServer.PeerBase.SendEvent(Photon.SocketServer.IEventData,Photon.SocketServer.SendParameters)">PhotonPeer.SendEvent</see>.
    ///The <see cref="T:Photon.SocketServer.IRpcProtocol"/> serializes EventData with <see cref="M:Photon.SocketServer.IRpcProtocol.SerializeEventData(Photon.SocketServer.EventData)">SerializeEventData</see>. 
    /// <see cref="M:Photon.SocketServer.EventData.SetParameters(System.Object)"/> converts properties that are flagged with <see cref="T:Photon.SocketServer.Rpc.DataMemberAttribute"/> into the <see cref="P:Photon.SocketServer.EventData.Parameters"/> dictionary.
    /// </summary>
    public sealed class EventData : IEventData
    {
        /// <summary>
        ///  Initializes a new instance of the <see cref="T:Photon.SocketServer.EventData"/> class.
        /// </summary>
        public EventData()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Photon.SocketServer.EventData"/> class.
        /// </summary>
        /// <param name="eventCode">The event Code.</param>
        public EventData(byte eventCode)
        {
            this.Code = eventCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Photon.SocketServer.EventData"/> class.
        /// </summary>
        /// <param name="eventCode">The event Code.</param>
        /// <param name="dataContract">All properties of <paramref name="dataContract"/> with the <see cref="T:Photon.SocketServer.Rpc.DataMemberAttribute"/> are mapped to the <see cref="P:Photon.SocketServer.EventData.Parameters"/> dictionary.</param>
        public EventData(byte eventCode, object dataContract)
        {
            this.Code = eventCode;
            this.SetParameters(dataContract);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Photon.SocketServer.EventData"/> class.
        /// This constructor sets the <see cref="P:Photon.SocketServer.EventData.Parameters"/> and the <see cref="P:Photon.SocketServer.EventData.Code"/>.
        /// </summary>
        /// <param name="eventCode"> Determines the <see cref="P:Photon.SocketServer.EventData.Code"/>.</param>
        /// <param name="parameters"> Determines the <see cref="P:Photon.SocketServer.EventData.Parameters"/>.</param>
        public EventData(byte eventCode, Dictionary<byte, object> parameters)
        {
            this.Code = eventCode;
            this.Parameters = parameters;
        }

        /// <summary>
        ///Sends the event to a list of peers.
        /// This method serializes the data just once per protocol instead of once per peer. 
        /// </summary>
        /// <typeparam name="TPeer">A <see cref="T:Photon.SocketServer.PeerBase"/> subclass type.</typeparam>
        /// <param name="peers"> The peers to send the event to.</param>
        /// <param name="sendParameters"> The send Options.</param>
        public void SendTo<TPeer>(IEnumerable<TPeer> peers, SendParameters sendParameters) where TPeer : PeerBase
        {
            SendTo<TPeer>(this, peers, sendParameters);
        }

        /// <summary>
        ///  Sends an event to a list of peers.
        /// This method serializes the data just once per protocol instead of once per peer.
        /// </summary>
        /// <typeparam name="TPeer">A <see cref="T:Photon.SocketServer.PeerBase"/> subclass type.</typeparam>
        /// <param name="eventData"> The event to send.</param>
        /// <param name="peers"> The peers to send the event to.</param>
        /// <param name="sendParameters"> The send Options.</param>
        public static void SendTo<TPeer>(IEventData eventData, IEnumerable<TPeer> peers, SendParameters sendParameters) where TPeer : PeerBase
        {
            if (eventData == null)
            {
                throw new ArgumentNullException("eventData");
            }
            if (peers == null)
            {
                throw new ArgumentNullException("peers");
            }
            ApplicationBase.Instance.BroadCastEvent<TPeer>(eventData, peers, sendParameters);
        }

        /// <summary>
        /// Serializes this instance with the <paramref name = "protocol" />.
        /// </summary>
        /// <param name="protocol">The protocol.</param>
        /// <returns> A byte array.</returns>
        public byte[] Serialize(IRpcProtocol protocol)
        {
            return protocol.SerializeEventData(this);
        }

        /// <summary>
        ///Converts properties of an object to <see cref="P:Photon.SocketServer.EventData.Parameters"/>.
        /// Included properties require the <see cref="T:Photon.SocketServer.Rpc.DataMemberAttribute"/>.
        /// </summary>
        /// <param name="dataContract">
        /// The properties of this object are mapped to <see cref="P:Photon.SocketServer.EventData.Parameters"/>.
        /// </param>
        public void SetParameters(object dataContract)
        {
            this.Parameters = (dataContract == null) ? null : ObjectDataMemberMapper.GetValues<DataMemberAttribute>(dataContract);
        }

        /// <summary>
        /// Replaces the <see cref="P:Photon.SocketServer.EventData.Parameters"/> with <paramref name="parameters"/>.
        /// </summary>
        /// <param name="parameters"> The parameters to set.</param>
        public void SetParameters(Dictionary<byte, object> parameters)
        {
            this.Parameters = parameters;
        }

        /// <summary>
        ///  Gets or sets the event code.
        /// </summary>
        public byte Code { get; set; }

        /// <summary>
        /// Gets or sets the paramter associated with the specified key.
        /// </summary>
        /// <param name="parameterKey">
        /// The key of the parameter to get or set.
        /// </param>
        /// <returns>The parameter associated with the specified key. 
        ///If the specified key is not found, a get operation throws a KeyNotFoundException, 
        ///and a set operation creates a new paramter with the specified key.
        ///</returns>
        ///<exception cref="T:System.NullReferenceException">
        ///The <see cref="P:Photon.SocketServer.EventData.Parameters"/> property has not been initialized.
        ///</exception>
        public object this[byte parameterKey]
        {
            get
            {
                return this.Parameters[parameterKey];
            }
            set
            {
                this.Parameters[parameterKey] = value;
            }
        }

        /// <summary>
        ///  Gets or sets the event parameters that will be sent to the client.
        /// </summary>
        public Dictionary<byte, object> Parameters { get; set; }
    }
}
