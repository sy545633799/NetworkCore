namespace Photon.SocketServer.Rpc.Protocols
{
    /// <summary>
    /// Defines the valid magic bytes 
    /// </summary>
    public enum RtsMagicByte : byte
    {
        /// <summary>
        /// The second header version for AMF3. 
        /// </summary>
        Amf3V2 = 0xf4,

        /// <summary>
        /// The second header version for GpBinary (photon 3) 
        /// </summary>
        GpBinaryV2 = 0xf3
    }
}
