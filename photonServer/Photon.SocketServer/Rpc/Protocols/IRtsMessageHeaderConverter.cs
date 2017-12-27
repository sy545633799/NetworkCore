using System.IO;

namespace Photon.SocketServer.Rpc.Protocols
{
    /// <summary>
    /// Implementors write the rts message headers to a data stream. 
    /// </summary>
    internal interface IRtsMessageHeaderConverter
    {
        /// <summary>
        /// Tries to parse the message header. 
        /// </summary>
        /// <param name="data">The input data. </param>
        /// <param name="header">The output header. </param>
        /// <returns>True on success. </returns>
        bool TryParseHeader(byte[] data, out RtsMessageHeader header);

        /// <summary>
        /// Writes the header to the stream at the current position. 
        /// </summary>
        /// <param name="stream">The stream. </param>
        /// <param name="messageType">The message type. </param>
        /// <param name="encrypted">Indiciates whether the message body is encrypted. </param>
        void WriteHeader(Stream stream, RtsMessageType messageType, bool encrypted);

        /// <summary>
        /// Gets the size of the message header. 
        /// </summary>
        byte HeaderSize { get; }
    }
}
