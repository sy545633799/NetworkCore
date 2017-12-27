using System.Runtime.InteropServices;

namespace Photon.SocketServer.Rpc.Protocols
{
    /// <summary>
    /// The rts message header. 
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct RtsMessageHeader
    {
        /// <summary>
        /// Gets or sets a value indicating whether the message is encrypted. 
        /// </summary>
        public bool IsEncrypted { get; set; }

        /// <summary>
        /// Gets or sets the message type. 
        /// </summary>
        public RtsMagicByte MagicByte { get; set; }

        /// <summary>
        /// Gets or sets the message type. 
        /// </summary>
        public RtsMessageType MessageType { get; set; }

        /// <summary>
        /// Gets or sets the size in bytes. 
        /// </summary>
        public byte SizeInBytes { get; set; }
    }
}
