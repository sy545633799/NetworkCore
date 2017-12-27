namespace Photon.SocketServer.Rpc.Protocols
{
    /// <summary>
    /// Specifies the available protocol types. 
    /// </summary>
    public enum ProtocolType
    {
        /// <summary>
        /// Binary byte protocol v1.5 with the new header. 
        /// </summary>
        GpBinaryV152,

        /// <summary>
        /// Binary byte protocol version 1.6 with the new header. 
        /// </summary>
        GpBinaryV162,

        /// <summary>
        /// Flash AMF3 protocol with the new header. 
        /// </summary>
        Amf3V152,

        /// <summary>
        /// Json protocol used by websockets. 
        /// </summary>
        Json,

        /// <summary>
        /// Binary byte protocol version 1.7 with the new header. 
        /// </summary>
        GpBinaryV17
    }
}
