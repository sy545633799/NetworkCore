namespace Photon.SocketServer.Rpc.Protocols
{
    /// <summary>
    /// Defines the RealTimeServer message types used for serialization. 
    /// </summary>
    public enum RtsMessageType : byte
    { 
        /// <summary>
        /// The Init mesasge. 
        /// </summary>
        Init = 0,

        /// <summary>
        /// The Init response message. 
        /// </summary>
        InitResponse = 1,
        
        /// <summary>
        /// The Operation request message. 
        /// </summary>
        Operation = 2,

        /// <summary>
        /// The Operation response message. 
        /// </summary>
        OperationResponse = 3,

        /// <summary>
        /// The Event message. 
        /// </summary>
        Event = 4,

        /// <summary>
        /// Message type for internal operation requests. 
        /// </summary>
        InternalOperationRequest = 6,

        /// <summary>
        /// Message type for internal operation responses.
        /// </summary>
        InternalOperationResponse = 7
    }
}
