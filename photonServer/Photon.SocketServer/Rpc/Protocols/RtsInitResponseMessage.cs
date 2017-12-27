using System.IO;

namespace Photon.SocketServer.Rpc.Protocols
{
    /// <summary>
    /// The rts init response message. 
    /// </summary>
    internal static class RtsInitResponseMessage
    {
        /// <summary>
        /// The size in bytes. 
        /// </summary>
        public static readonly int SizeInBytes = 1;

        /// <summary>
        /// The serialize. 
        /// </summary>
        /// <param name="stream">The stream. </param>
        public static void Serialize(Stream stream)
        {
            stream.WriteByte(0);
        }

        /// <summary>
        /// The try parse. 
        /// </summary>
        /// <param name="data">The data. </param>
        /// <param name="index">The index. </param>
        /// <returns>true if successful. </returns>
        public static bool TryParse(byte[] data, int index)
        {
            return (data[index] == 0);
        }
    }
}
