using System.Collections.Generic;

namespace Photon.SocketServer
{
    /// <summary>
    /// An <see cref="T:Photon.SocketServer.IEventData"/> wrapper that serializes the event upon creation.
    /// This has a performance benefit if multiple receivers use the same protocol.
    /// </summary>
    public sealed class SerializedEventData : IEventData
    {
        /// <summary>
        /// The original event data.
        /// </summary>
        private readonly IEventData eventData;

        /// <summary>
        /// The used serialization protocol.
        /// </summary>
        private readonly IRpcProtocol rpcProtocol;

        /// <summary>
        /// The cached serialzed data.
        /// </summary>
        private readonly byte[] serializedData;

        /// <summary>
        ///  Initializes a new instance of the <see cref="T:Photon.SocketServer.SerializedEventData"/> class.
        /// </summary>
        /// <param name="eventData">The event data.</param>
        /// <param name="rpcProtocol">The rpc protocol.</param>
        public SerializedEventData(IEventData eventData, IRpcProtocol rpcProtocol)
        {
            this.eventData = eventData;
            this.rpcProtocol = rpcProtocol;
            this.serializedData = eventData.Serialize(rpcProtocol);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Photon.SocketServer.SerializedEventData"/> class.
        /// </summary>
        /// <param name="eventData">The event data.</param>
        /// <param name="rpcProtocol">The rpc protocol.</param>
        /// <param name="data"> The data.</param>
        internal SerializedEventData(IEventData eventData, IRpcProtocol rpcProtocol, byte[] data)
        {
            this.eventData = eventData;
            this.rpcProtocol = rpcProtocol;
            this.serializedData = data;
        }

        /// <summary>
        /// Returns the cached serialization data if the protocol is matches, otherwise serializes with the <paramref name = "protocol" />.
        /// </summary>
        /// <param name="protocol">The protocol.</param>
        /// <returns>A byte array.</returns>
        public byte[] Serialize(IRpcProtocol protocol)
        {
            if (protocol == this.rpcProtocol)
            {
                return this.serializedData;
            }
            return this.eventData.Serialize(protocol);
        }

        /// <summary>
        ///  Gets the event code.
        /// </summary>
        public byte Code
        {
            get
            {
                return this.eventData.Code;
            }
        }

        /// <summary>
        ///  Gets the event data.
        /// </summary>
        internal byte[] Data
        {
            get
            {
                return this.serializedData;
            }
        }

        /// <summary>
        ///   Gets the serialized data.
        /// </summary>
        public IEventData EventData
        {
            get
            {
                return this.eventData;
            }
        }

        /// <summary>
        /// Gets or sets the paramter associated with the specified key.
        /// </summary>
        /// <param name="parameterKey">
        /// The key of the parameter to get or set.
        /// </param>
        /// <returns>
        /// The parameter associated with the specified key. 
        ///If the specified key is not found, a get operation throws a KeyNotFoundException, 
        ///and a set operation creates a new paramter with the specified key.
        ///</returns>
        ///<exception cref="T:System.NullReferenceException">
        /// The <see cref="P:Photon.SocketServer.SerializedEventData.Parameters"/> property has not been initialized.
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
        /// Gets the event params.
        /// </summary>
        public Dictionary<byte, object> Parameters
        {
            get
            {
                return this.eventData.Parameters;
            }
        }

        /// <summary>
        /// Gets the used serialization protocol.
        /// </summary>
        public IRpcProtocol Protocol
        {
            get
            {
                return this.rpcProtocol;
            }
        }
    }
}
