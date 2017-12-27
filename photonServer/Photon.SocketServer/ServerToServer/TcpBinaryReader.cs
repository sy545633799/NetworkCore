using System;
using ExitGames.Logging;
using Photon.SocketServer.Security;

namespace Photon.SocketServer.ServerToServer
{
    /// <summary>
    /// The tcp binary reader.
    /// </summary>
    internal class TcpBinaryReader
    {
        /// <summary>
        /// The header size.
        /// </summary>
        private const byte HeaderSize = 7;

        /// <summary>
        /// An <see cref="T:ExitGames.Logging.ILogger"/> instance used to log messages to the logging framework.
        /// </summary>
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The header buffer.
        /// </summary>
        private ByteBuffer headerBuffer;

        /// <summary>
        ///  The message buffer.
        /// </summary>
        private ByteBuffer messageBuffer;

        /// <summary>
        /// The parse function.
        /// </summary>
        private ParseDelegate parseFunction;

        /// <summary>
        /// The ping response buffer.
        /// </summary>
        private ByteBuffer pingResponseBuffer;

        /// <summary>
        /// The received send parameters for channelId and reliability
        /// </summary>
        private SendParameters sendParameters;

        /// <summary>
        ///  Invoked if a message was received.
        /// </summary>
        public event Action<byte[], SendParameters> OnDataReceived;

        /// <summary>
        /// Invoked if a <see cref="T:Photon.SocketServer.ServerToServer.PingResponse"/> was received.
        /// </summary>
        public event Action<PingResponse> OnPingResponse;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Photon.SocketServer.ServerToServer.TcpBinaryReader"/> class.
        /// </summary>
        public TcpBinaryReader()
        {
            this.parseFunction = new ParseDelegate(this.ParseMagicNumber);
        }

        public void Parse(byte[] buffer, int count)
        {
            for (int i = 0; i < count; i += this.parseFunction(buffer, i, count))
            {
            }
        }

        private int ParseHeader(byte[] buffer, int offset, int count)
        {
            int num = this.headerBuffer.Read(buffer, offset, count);
            if (this.headerBuffer.Complete)
            {
                byte[] buffer2 = this.headerBuffer.Buffer;
                int num2 = (((buffer2[1] << 0x18) | (buffer2[2] << 0x10)) | (buffer2[3] << 8)) | buffer2[4];
                this.sendParameters.ChannelId = buffer2[5];
                this.sendParameters.Unreliable = buffer2[6] == 0;
                this.messageBuffer = new ByteBuffer(num2 - 7);
                this.parseFunction = new ParseDelegate(this.ParseMessage);
            }
            return num;
        }

        /// <summary>
        /// The parse magic number.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="count"> The count.</param>
        /// <returns>The parsed magic number.</returns>
        private int ParseMagicNumber(byte[] buffer, int offset, int count)
        {
            switch (buffer[offset])
            {
                case 240:
                    this.pingResponseBuffer = new ByteBuffer(PingResponse.SizeInBytes);
                    this.parseFunction = new ParseDelegate(this.ParsePing);
                    break;

                case 0xfb:
                    this.headerBuffer = new ByteBuffer(7);
                    this.parseFunction = new ParseDelegate(this.ParseHeader);
                    return 0;

                default:
                    if (log.IsDebugEnabled)
                    {
                        log.DebugFormat("Received invalid data: {0}", new object[] { BitConverter.ToString(buffer, offset) });
                    }
                    break;
            }
            return 1;
        }

        /// <summary>
        ///  The parse message.
        /// </summary>
        /// <param name="buffer"> The buffer.</param>
        /// <param name="offset"> The offset.</param>
        /// <param name="count"> The count.</param>
        /// <returns> Number of bytes parsed.</returns>
        private int ParseMessage(byte[] buffer, int offset, int count)
        {
            int num = this.messageBuffer.Read(buffer, offset, count);
            if (this.messageBuffer.Complete)
            {
                Action<byte[], SendParameters> onDataReceived = this.OnDataReceived;
                if (onDataReceived != null)
                {
                    onDataReceived(this.messageBuffer.Buffer, this.sendParameters);
                }
                this.parseFunction = new ParseDelegate(this.ParseMagicNumber);
            }
            return num;
        }

        /// <summary>
        /// The parse ping.
        /// </summary>
        /// <param name="buffer"> The buffer.</param>
        /// <param name="offset"> The offset.</param>
        /// <param name="count">The count.</param>
        /// <returns>Number of bytes parsed.</returns>
        private int ParsePing(byte[] buffer, int offset, int count)
        {
            int num = this.pingResponseBuffer.Read(buffer, offset, count);
            if (this.pingResponseBuffer.Complete)
            {
                PingResponse response = new PingResponse(this.pingResponseBuffer.Buffer);
                Action<PingResponse> onPingResponse = this.OnPingResponse;
                if (onPingResponse != null)
                {
                    onPingResponse(response);
                }
                this.parseFunction = new ParseDelegate(this.ParseMagicNumber);
            }
            return num;
        }

        /// <summary>
        ///  Gets or sets the CryptoProvider.
        /// </summary>
        public ICryptoProvider CryptoProvider { get; set; }

        /// <summary>
        /// The parse delegate.
        /// </summary>
        /// <param name="buffer"> The buffer.</param>
        /// <param name="offset"> The offset.</param>
        /// <param name="count">The count.</param>
        /// <returns> number of bytes parsed</returns>
        private delegate int ParseDelegate(byte[] buffer, int offset, int count);
    }
}
