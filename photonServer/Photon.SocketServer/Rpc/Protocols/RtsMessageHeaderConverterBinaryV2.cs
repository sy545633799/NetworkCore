using System;
using System.IO;
using ExitGames.IO;
using ExitGames.Logging;

namespace Photon.SocketServer.Rpc.Protocols
{
    /// <summary>
    /// This <see cref="T:Photon.SocketServer.Rpc.Protocols.IRtsMessageHeaderConverter"/> writes a 2 bytes long header and uses magic byte 0xF3.
    /// </summary>
    internal class RtsMessageHeaderConverterBinaryV2 : IRtsMessageHeaderConverter
    {
        /// <summary>
        ///  Singleton instance.
        /// </summary>
        public static readonly RtsMessageHeaderConverterBinaryV2 Instance = new RtsMessageHeaderConverterBinaryV2();

        /// <summary>
        ///  For logging.
        /// </summary>
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Prevents a default instance of the <see cref="T:Photon.SocketServer.Rpc.Protocols.RtsMessageHeaderConverterBinaryV2"/> class from being created.
        /// </summary>
        private RtsMessageHeaderConverterBinaryV2()
        {
        }

        /// <summary>
        /// Tries to parse the message header.
        /// </summary>
        /// <param name="data">The input data.</param>
        /// <param name="header">The output header.</param>
        /// <returns>True on success.</returns>
        public unsafe bool TryParseHeader(byte[] data, out RtsMessageHeader header)
        {
            if (data.Length > 1)
            {
                fixed (byte* numRef = data)
                {
                    if (numRef[0] == 0xf3)
                    {
                        byte num = numRef[1];
                        RtsMessageHeader header2 = new RtsMessageHeader
                        {
                            SizeInBytes = this.HeaderSize,
                            MessageType = (RtsMessageType)((byte)(num & 0x7f)),
                            IsEncrypted = (num & 0x80) == 0x80,
                            MagicByte = RtsMagicByte.GpBinaryV2
                        };
                        header = header2;
                        return true;
                    }
                }
            }
            if (log.IsDebugEnabled)
            {
                log.DebugFormat("Failed to parse message header: {0}", new object[] { BitConverter.ToString(data, 0, Math.Max(this.HeaderSize, data.Length)) });
            }
            header = new RtsMessageHeader();
            return false;
        }

        /// <summary>
        /// Writes the header to the stream at the current position.
        /// </summary>
        /// <param name="stream"> The stream.</param>
        /// <param name="messageType"> The message type.</param>
        /// <param name="encrypted">Indiciates whether the message body is encrypted.</param>
        public void WriteHeader(Stream stream, RtsMessageType messageType, bool encrypted)
        {
            BigEndianBinaryWriter.WriteByte(stream, 0xf3);
            byte num = (byte)messageType;
            if (encrypted)
            {
                num = (byte)(num | 0x80);
            }
            BigEndianBinaryWriter.WriteByte(stream, num);
        }

        /// <summary>
        /// Gets the size of the message header - in this case 2.
        /// </summary>
        public byte HeaderSize
        {
            get
            {
                return 2;
            }
        }
    }
}
