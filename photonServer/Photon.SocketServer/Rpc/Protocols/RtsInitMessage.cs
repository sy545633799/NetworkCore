using System;
using System.IO;
using System.Text;

namespace Photon.SocketServer.Rpc.Protocols
{
    /// <summary>
    /// Represents a realtime server intitialization message.
    /// </summary>
    internal sealed class RtsInitMessage
    {
        /// <summary>
        /// The application id.
        /// </summary>
        private readonly string applicationId;

        /// <summary>
        /// The client version.
        /// </summary>
        private readonly byte[] clientVersion;

        /// <summary>
        /// Gets the protocol version.
        /// </summary>
        private readonly byte[] protocolVersion;

        /// <summary>
        /// Returns the size of the <see cref="T:Photon.SocketServer.Rpc.Protocols.RtsMessageHeader"/> in bytes.
        /// </summary>
        public static readonly int SizeInBytes = 0x27;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Photon.SocketServer.Rpc.Protocols.RtsInitMessage"/> class.
        /// </summary>
        /// <param name="protocolVersion">The protocol version.</param>
        /// <param name="clientVersion">The client version.</param>
        /// <param name="applicationId">The application Id.</param>
        public RtsInitMessage(byte[] protocolVersion, byte[] clientVersion, string applicationId)
        {
            this.protocolVersion = protocolVersion;
            this.clientVersion = clientVersion;
            this.applicationId = applicationId;
        }

        /// <summary>
        /// The serialize.
        /// </summary>
        /// <param name="stream"> The stream.</param>
        public void Serialize(Stream stream)
        {
            stream.WriteByte(this.protocolVersion[0]);
            stream.WriteByte(this.protocolVersion[1]);
            stream.WriteByte(1);
            stream.WriteByte(this.clientVersion[0]);
            stream.WriteByte(this.clientVersion[1]);
            stream.WriteByte(this.clientVersion[2]);
            stream.WriteByte(7);
            byte[] bytes = new byte[0x20];
            Encoding.ASCII.GetBytes(this.applicationId, 0, Math.Min(this.applicationId.Length, 0x20), bytes, 0);
            stream.Write(bytes, 0, 0x20);
        }

        /// <summary>
        /// Tries to parse a <see cref="T:Photon.SocketServer.Rpc.Protocols.RtsInitMessage"/> from a byte array.
        /// </summary>
        /// <param name="buffer"> The buffer.</param>
        /// <param name="startIndex">The start index.</param>
        /// <param name="initMessage"> The init message.</param>
        /// <returns>true if successfull.</returns>
        public static bool TryParse(byte[] buffer, int startIndex, out RtsInitMessage initMessage)
        {
            initMessage = null;
            if (buffer.Length < (startIndex + 0x27))
            {
                return false;
            }
            byte[] protocolVersion = new byte[] { buffer[startIndex], buffer[startIndex + 1] };
            int index = startIndex + 3;
            byte[] clientVersion = new byte[] { buffer[index], buffer[index + 1], buffer[index + 2] };
            int num2 = index + 4;
            string applicationId = Encoding.ASCII.GetString(buffer, num2, 0x20);
            int length = applicationId.IndexOf('\0');
            if (length > -1)
            {
                applicationId = applicationId.Substring(0, length);
            }
            initMessage = new RtsInitMessage(protocolVersion, clientVersion, applicationId);
            return true;
        }

        /// <summary>
        /// Gets ApplicationId.
        /// </summary>
        public string ApplicationId
        {
            get
            {
                return this.applicationId;
            }
        }

        /// <summary>
        /// Gets ClientVersion.
        /// </summary>
        public byte[] ClientVersion
        {
            get
            {
                return this.clientVersion;
            }
        }

        /// <summary>
        ///  Gets ProtocolVersion.
        /// </summary>
        public byte[] ProtocolVersion
        {
            get
            {
                return this.protocolVersion;
            }
        }
    }
}
