using System;
using ExitGames.Logging;
using Photon.SocketServer.ServerToServer;
using PhotonHostRuntimeInterfaces;

namespace Photon.SocketServer.Web
{
    internal class TcpReader
    {
        // Fields
        private ByteBuffer headerBuffer;
        private const byte HeaderSize = 7;
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();
        private ByteBuffer messageBuffer;
        private ParseDelegate parseFunction;
        private ByteBuffer pingResponseBuffer;
        private readonly ITcpListener tcpListener;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Photon.SocketServer.Web.TcpReader"/> class.
        /// </summary>
        /// <param name="tcpListener"></param>
        internal TcpReader(ITcpListener tcpListener)
        {
            this.tcpListener = tcpListener;
            this.parseFunction = new ParseDelegate(this.ParseMagicNumber);
        }

        /// <summary>
        /// The parse.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="count">The count.</param>
        public void Parse(byte[] buffer, int count)
        {
            for (int i = 0; i < count; i += this.parseFunction(buffer, i, count))
            {
            }
        }

        /// <summary>
        /// The parse header.
        /// </summary>
        /// <param name="buffer"> The buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="count">The count.</param>
        /// <returns>Number of bytes parsed.</returns>
        private int ParseHeader(byte[] buffer, int offset, int count)
        {
            int num = this.headerBuffer.Read(buffer, offset, count);
            if (this.headerBuffer.Complete)
            {
                byte[] buffer2 = this.headerBuffer.Buffer;
                int num2 = (((buffer2[1] << 0x18) | (buffer2[2] << 0x10)) | (buffer2[3] << 8)) | buffer2[4];
                this.messageBuffer = new ByteBuffer(num2 - 7);
                this.parseFunction = new ParseDelegate(this.ParseMessage);
            }
            return num;
        }

        /// <summary>
        /// The parse magic number.
        /// </summary>
        /// <param name="buffer"> The buffer.</param>
        /// <param name="offset">   The offset.</param>
        /// <param name="count">The count.</param>
        /// <returns> The parsed magic number.</returns>
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
        /// <param name="count">The count.</param>
        /// <returns>Number of bytes parsed.</returns>
        private int ParseMessage(byte[] buffer, int offset, int count)
        {
            int num = this.messageBuffer.Read(buffer, offset, count);
            if (this.messageBuffer.Complete)
            {
                byte channelId = this.headerBuffer.Buffer[5];
                MessageReliablity reliablity = (this.headerBuffer.Buffer[6] == 0) ? MessageReliablity.UnReliable : MessageReliablity.Reliable;
                this.tcpListener.OnReceive(this.messageBuffer.Buffer, channelId, reliablity);
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
                int serverTime = BitConverter.ToInt32(buffer, 0);
                int clienttime = BitConverter.ToInt32(buffer, 4);
                this.tcpListener.OnPingResponse(serverTime, clienttime);
                this.parseFunction = new ParseDelegate(this.ParseMagicNumber);
            }
            return num;
        }

        /// <summary>
        /// The parse delegate.
        /// </summary>
        /// <param name="buffer"> The buffer.</param>
        /// <param name="offset"> The offset.</param>
        /// <param name="count">The count.</param>
        /// <returns>number of bytes parsed</returns>
        private delegate int ParseDelegate(byte[] buffer, int offset, int count);
    }
}
