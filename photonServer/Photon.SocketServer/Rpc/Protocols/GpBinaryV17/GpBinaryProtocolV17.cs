using System;
using System.IO;
using ExitGames.Logging;
using Photon.SocketServer.Rpc.Reflection;
using Photon.SocketServer.Security;

namespace Photon.SocketServer.Rpc.Protocols.GpBinaryV17
{
    /// <summary>
    /// The GpBinary implementation of <see cref="T:Photon.SocketServer.IRpcProtocol"/>.
    /// </summary>
    internal class GpBinaryProtocolV17 : IRpcProtocol
    {
        // Fields
        private const int HeaderSize = 2;
        public static readonly GpBinaryProtocolV17 Instance = new GpBinaryProtocolV17();

        /// <summary>
        /// An <see cref="T:ExitGames.Logging.ILogger"/> instance used to log operations to the logging framework.
        /// </summary>
        private static readonly ILogger operationDataLogger = LogManager.GetLogger("OperationData");

        /// <summary>
        /// The protocol type
        /// </summary>
        private const ProtocolType Protocol = ProtocolType.GpBinaryV17;

        /// <summary>
        /// The protocol version 1.7
        /// </summary>
        public static readonly Version ProtocolVersion = new Version(1, 7);

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Photon.SocketServer.Rpc.Protocols.GpBinaryV17.GpBinaryProtocolV17"/> class.
        /// </summary>
        protected GpBinaryProtocolV17()
        {
        }

        /// <summary>
        /// Serialze an object to a stream
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="obj"> The object to serialize.</param>
        public void Serialize(Stream stream, object obj)
        {
            GpBinaryByteWriterV17.Write(stream, obj);
        }

        /// <summary>
        ///  The serialize event.
        /// </summary>
        /// <param name="eventData">The event data.</param>
        /// <returns>The serialized event.</returns>               
        /// <exception cref="T:System.IO.InvalidDataException">
        ///A value can not be serialized.
        ///</exception>
        ///<exception cref="T:System.ArrayTypeMismatchException">
        /// A collection with different types can not be serialized.
        ///</exception>
        public byte[] SerializeEventData(EventData eventData)
        {
            using (ReusableMemoryStream stream = new ReusableMemoryStream())
            {
                stream.WriteByte(0xf3);
                stream.WriteByte(4);
                SerializeEventData(stream, eventData);
                return stream.ToArray();
            }
        }

        /// <summary>
        /// serializes the event data to a stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="eventData"> The event data.</param>
        private static void SerializeEventData(Stream stream, IEventData eventData)
        {
            GpBinaryByteWriterV17.WriteEventData(stream, eventData);
        }

        /// <summary>
        /// Encrypts an event.
        /// </summary>
        /// <param name="eventData"> The event data.</param>
        /// <param name="cryptoProvider">The crypto provider.</param>
        /// <returns>the encrypted event.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// cryptoProvider is null.
        /// </exception>
        public byte[] SerializeEventDataEncrypted(IEventData eventData, ICryptoProvider cryptoProvider)
        {
            byte[] buffer;
            if (cryptoProvider == null)
            {
                throw new ArgumentNullException("cryptoProvider");
            }
            using (ReusableMemoryStream stream = new ReusableMemoryStream())
            {
                SerializeEventData(stream, eventData);
                byte[] data = stream.ToArray();
                buffer = cryptoProvider.Encrypt(data, 0, data.Length);
            }
            using (ReusableMemoryStream stream2 = new ReusableMemoryStream())
            {
                stream2.WriteByte(0xf3);
                stream2.WriteByte(0x84);
                stream2.Write(buffer, 0, buffer.Length);
                return stream2.ToArray();
            }
        }

        /// <summary>
        /// The serialize init request.
        /// </summary>
        /// <param name="applicationId">The application id.</param>
        /// <param name="clientVersion">The client version.</param>
        /// <returns>a serialized init request message</returns>
        public byte[] SerializeInitRequest(string applicationId, Version clientVersion)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                stream.WriteByte(0xf3);
                stream.WriteByte(0);
                new RtsInitMessage(new byte[] { (byte)ProtocolVersion.Major, (byte)ProtocolVersion.Minor }, new byte[] { (byte)clientVersion.Major, (byte)clientVersion.Minor, (byte)clientVersion.Build }, applicationId).Serialize(stream);
                return stream.ToArray();
            }
        }

        /// <summary>
        /// The serialize init response.
        /// </summary>
        /// <returns>The serialized init response.</returns>
        public byte[] SerializeInitResponse()
        {
            byte[] buffer = new byte[3];
            buffer[0] = 0xf3;
            buffer[1] = 1;
            return buffer;
        }

        /// <summary>
        /// Serializes an internal <see cref="T:Photon.SocketServer.OperationRequest"/>.
        /// </summary>
        /// <param name="operationRequest"> The operation request.</param>
        /// <returns>The serialized operation request.</returns>
        public byte[] SerializeInternalOperationRequest(OperationRequest operationRequest)
        {
            return this.SerializeOperationRequest(operationRequest, RtsMessageType.InternalOperationRequest);
        }

        /// <summary>
        /// Serializes an internal <see cref="T:Photon.SocketServer.OperationResponse"/>.
        /// </summary>
        /// <param name="operationResponse">The operation response.</param>
        /// <returns>The serialized operation response.</returns>
        public byte[] SerializeInternalOperationResponse(OperationResponse operationResponse)
        {
            return this.SerializeOperationResponse(operationResponse, RtsMessageType.InternalOperationResponse);
        }

        /// <summary>
        /// Serializes an operation request.
        /// </summary>
        /// <param name="operationRequest">The operation request.</param>
        /// <returns>a serialized operation request message</returns>
        public byte[] SerializeOperationRequest(OperationRequest operationRequest)
        {
            using (ReusableMemoryStream stream = new ReusableMemoryStream())
            {
                stream.WriteByte(0xf3);
                stream.WriteByte(2);
                SerializeOperationRequest(stream, operationRequest);
                return stream.ToArray();
            }
        }

        /// <summary>
        ///  Serializes an <see cref="T:Photon.SocketServer.OperationResponse"/>.
        /// </summary>
        /// <param name="operationRequest">The operation request to serialize.</param>
        /// <param name="messageType">
        /// The message type. 
        ///Should be eiter <see c
        ///ref="F:Photon.SocketServer.Rpc.Protocols.RtsMessageType.OperationResponse"/> or <see
        ///cref="F:Photon.SocketServer.Rpc.Protocols.RtsMessageType.InternalOperationResponse"/>.
        /// </param>
        /// <returns>A serialized operation response.</returns>
        private byte[] SerializeOperationRequest(OperationRequest operationRequest, RtsMessageType messageType)
        {
            using (ReusableMemoryStream stream = new ReusableMemoryStream())
            {
                stream.WriteByte(0xf3);
                stream.WriteByte((byte)messageType);
                SerializeOperationRequest(stream, operationRequest);
                return stream.ToArray();
            }
        }

        /// <summary>
        /// Serializes an operation request.
        /// </summary>
        /// <param name="stream"> The stream.</param>
        /// <param name="operationRequest">The operation request.</param>                    
        /// <exception cref="T:System.IO.InvalidDataException">
        ///A value can not be serialized.
        ///</exception>
        ///<exception cref="T:System.ArrayTypeMismatchException">
        /// A collection with different types can not be serialized.
        ///</exception>
        private static void SerializeOperationRequest(Stream stream, OperationRequest operationRequest)
        {
            GpBinaryByteWriterV17.WriteOperationRequest(stream, operationRequest);
        }

        /// <summary>
        /// The serialize operation request.
        /// </summary>
        /// <param name="operationRequest"> The operation request.</param>
        /// <param name="cryptoProvider">The <see cref="T:System.Security.Cryptography.ICryptoTransform"/> used to encrypt operation response data.</param>
        /// <returns> a serialized operation request message</returns>
        public byte[] SerializeOperationRequestEncrypted(OperationRequest operationRequest, ICryptoProvider cryptoProvider)
        {
            byte[] buffer;
            if (cryptoProvider == null)
            {
                throw new ArgumentNullException("cryptoProvider");
            }
            using (ReusableMemoryStream stream = new ReusableMemoryStream())
            {
                SerializeOperationRequest(stream, operationRequest);
                byte[] data = stream.ToArray();
                buffer = cryptoProvider.Encrypt(data, 0, data.Length);
            }
            using (ReusableMemoryStream stream2 = new ReusableMemoryStream())
            {
                stream2.WriteByte(0xf3);
                stream2.WriteByte(130);
                stream2.Write(buffer, 0, buffer.Length);
                return stream2.ToArray();
            }
        }

        /// <summary>
        ///  Serializes an <see cref="T:Photon.SocketServer.OperationResponse"/>.
        /// </summary>
        /// <param name="operationResponse">The response.</param>
        /// <returns>The serialized operation response.</returns>
        public byte[] SerializeOperationResponse(OperationResponse operationResponse)
        {
            return this.SerializeOperationResponse(operationResponse, RtsMessageType.OperationResponse);
        }

        /// <summary>
        /// Serializes an <see cref="T:Photon.SocketServer.OperationResponse"/>.
        /// </summary>
        /// <param name="operationResponse">The operation response to serialize.</param>
        /// <param name="messageType">
        ///  The message type. 
        /// Should be eiter <see 
        /// cref="F:Photon.SocketServer.Rpc.Protocols.RtsMessageType.OperationResponse"/> or <see
        /// cref="F:Photon.SocketServer.Rpc.Protocols.RtsMessageType.InternalOperationResponse"/>.
        /// </param>
        /// <returns>A serialized operation response.</returns>
        private byte[] SerializeOperationResponse(OperationResponse operationResponse, RtsMessageType messageType)
        {
            using (ReusableMemoryStream stream = new ReusableMemoryStream())
            {
                stream.WriteByte(0xf3);
                stream.WriteByte((byte)messageType);
                SerializeOperationResponse(stream, operationResponse);
                return stream.ToArray();
            }
        }

        /// <summary>
        /// Serializes an operation response.
        /// </summary>
        /// <param name="stream"> The stream.</param>
        /// <param name="operationResponse">The operation response.</param>
        /// <exception cref="T:System.IO.InvalidDataException">
        /// A value can not be serialized.
        ///</exception>
        ///<exception cref="T:System.ArrayTypeMismatchException">
        ///  A collection with different types can not be serialized.
        ///</exception>
        private static void SerializeOperationResponse(Stream stream, OperationResponse operationResponse)
        {
            GpBinaryByteWriterV17.WriteOperationResponse(stream, operationResponse);
        }

        /// <summary>
        /// Serializes an <see cref="T:Photon.SocketServer.OperationResponse"/>.
        /// The operation response data will be encrypted using the specified <see cref="T:Photon.SocketServer.Security.ICryptoProvider"/>.
        /// </summary>
        /// <param name="operationResponse"> The response.</param>
        /// <param name="cryptoProvider">An <see cref="T:Photon.SocketServer.Security.ICryptoProvider"/> instance used to encrypt the operation response.</param>
        /// <returns>The serialized operation response.</returns>
        public byte[] SerializeOperationResponseEncrypted(OperationResponse operationResponse, ICryptoProvider cryptoProvider)
        {
            byte[] buffer;
            if (cryptoProvider == null)
            {
                throw new ArgumentNullException("cryptoProvider");
            }
            using (ReusableMemoryStream stream = new ReusableMemoryStream())
            {
                SerializeOperationResponse(stream, operationResponse);
                byte[] data = stream.ToArray();
                buffer = cryptoProvider.Encrypt(data);
            }
            using (ReusableMemoryStream stream2 = new ReusableMemoryStream())
            {
                stream2.WriteByte(0xf3);
                stream2.WriteByte(0x83);
                stream2.Write(buffer, 0, buffer.Length);
                return stream2.ToArray();
            }
        }

        /// <summary>
        /// The convert operation parameter.
        /// </summary>
        /// <param name="paramterInfo">The paramter info.</param>
        /// <param name="value"> The value.</param>
        /// <returns><paramref name = "value" /> or a Guid if value is 16 bytes.</returns>
        public virtual bool TryConvertParameter(ObjectMemberInfo<DataMemberAttribute> paramterInfo, ref object value)
        {
            if (paramterInfo.ValueType == typeof(Guid))
            {
                if (value is Guid)
                {
                    return true;
                }
                byte[] buffer = value as byte[];
                if ((buffer != null) && (buffer.Length == 0x10))
                {
                    value = new Guid(buffer);
                    return true;
                }
                string str = value as string;
                if (str != null)
                {
                    value = new Guid(str);
                    return true;
                }
                return false;
            }
            if (paramterInfo.ValueType != typeof(Guid?))
            {
                return true;
            }
            if (value is Guid?)
            {
                return true;
            }
            byte[] b = value as byte[];
            if ((b != null) && (b.Length == 0x10))
            {
                value = new Guid(b);
                return true;
            }
            string g = value as string;
            if (g != null)
            {
                value = new Guid(g);
                return true;
            }
            return false;
        }

        /// <summary>
        ///  Try to parse an object from a stream.
        /// </summary>
        /// <param name="stream"> The stream.</param>
        /// <param name="obj"> The result object.</param>
        /// <returns>True on success.</returns>
        public bool TryParse(Stream stream, out object obj)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        ///  Tries to parse an event.
        /// </summary>
        /// <param name="data"> The data.</param>
        /// <param name="eventData"> The event data.</param>
        /// <returns>true if successful.</returns>
        public bool TryParseEventData(byte[] data, out EventData eventData)
        {
            object obj2;
            int offset = 2;
            if (GpBinaryByteReaderV17.ReadEventData(data, ref offset, out obj2))
            {
                eventData = (EventData)obj2;
                return true;
            }
            eventData = null;
            return false;
        }

        /// <summary>
        ///  Tries to parse an event.
        /// </summary>
        /// <param name="data"> The data.</param>
        /// <param name="cryptoProvider"> The crypto Provider.</param>
        /// <param name="eventData">The event data.</param>
        /// <returns>true if successful.</returns>
        public bool TryParseEventDataEncrypted(byte[] data, ICryptoProvider cryptoProvider, out EventData eventData)
        {
            object obj2;
            if (cryptoProvider == null)
            {
                eventData = null;
                return false;
            }
            byte[] buffer = cryptoProvider.Decrypt(data, 2, data.Length - 2);
            if (operationDataLogger.IsDebugEnabled)
            {
                operationDataLogger.DebugFormat("Decrypted data: data=({0} bytes) {1}", new object[] { buffer.Length, BitConverter.ToString(buffer) });
            }
            int offset = 0;
            if (GpBinaryByteReaderV17.ReadEventData(buffer, ref offset, out obj2))
            {
                eventData = (EventData)obj2;
                return true;
            }
            eventData = null;
            return false;
        }

        /// <summary>
        ///  Tries to parse the message header.
        /// </summary>
        /// <param name="data"> The data.</param>
        /// <param name="header"> The header.</param>
        /// <returns>True on success.</returns>
        public bool TryParseMessageHeader(byte[] data, out RtsMessageHeader header)
        {
            if ((data.Length < 2) | (data[0] != 0xf3))
            {
                header = new RtsMessageHeader();
                return false;
            }
            RtsMessageHeader header2 = new RtsMessageHeader
            {
                MagicByte = RtsMagicByte.GpBinaryV2,
                MessageType = (RtsMessageType)((byte)(data[1] & 0x7f)),
                IsEncrypted = (data[1] & 0x80) == 0x80
            };
            header = header2;
            return true;
        }

        /// <summary>
        ///  Tries to parse an <see cref="T:Photon.SocketServer.OperationRequest"/>.
        /// </summary>
        /// <param name="data"> The raw request data.</param>
        /// <param name="operationRequest">The operation request.</param>
        /// <returns>True if request was parsed successfully.</returns>
        public bool TryParseOperationRequest(byte[] data, out OperationRequest operationRequest)
        {
            object obj2;
            int offset = 2;
            if (GpBinaryByteReaderV17.ReadOperationRequest(data, ref offset, out obj2))
            {
                operationRequest = (OperationRequest)obj2;
                return true;
            }
            operationRequest = null;
            return false;
        }

        /// <summary>
        /// Tries to parse an <see cref="T:Photon.SocketServer.OperationRequest"/>.
        /// </summary>
        /// <param name="data">The raw request data.</param>
        /// <param name="cryptoProvider">An <see cref="T:Photon.SocketServer.Security.ICryptoProvider"/> instance used to decrypt encrypted operation requests.</param>
        /// <param name="operationRequest">The operation request.</param>
        /// <returns>True if request was parsed successfully.</returns>
        public bool TryParseOperationRequestEncrypted(byte[] data, ICryptoProvider cryptoProvider, out OperationRequest operationRequest)
        {
            object obj2;
            if (cryptoProvider == null)
            {
                operationRequest = null;
                return false;
            }
            byte[] buffer = cryptoProvider.Decrypt(data, 2, data.Length - 2);
            if (buffer == null)
            {
                operationRequest = null;
                return false;
            }
            if (operationDataLogger.IsDebugEnabled)
            {
                operationDataLogger.DebugFormat("Decrypted data: data=({0} bytes) {1}", new object[] { buffer.Length, BitConverter.ToString(buffer) });
            }
            int offset = 0;
            if (GpBinaryByteReaderV17.ReadOperationRequest(buffer, ref offset, out obj2))
            {
                operationRequest = (OperationRequest)obj2;
                return true;
            }
            operationRequest = null;
            return false;
        }

        /// <summary>
        ///  Tries to parse an operation response.
        /// </summary>
        /// <param name="data">A byte array containing the binary operation response data.</param>
        /// <param name="operationResponse">Contains the parsed operation response, if the methods returns with success;
        ///otherwise, the parameter will be uninitialized. </param>
        /// <returns>true if the operation response was parsed successfully; otherwise false.</returns>
        public bool TryParseOperationResponse(byte[] data, out OperationResponse operationResponse)
        {
            object obj2;
            int pos = 2;
            if (GpBinaryByteReaderV17.TryReadOperationResponse(data, ref pos, out obj2))
            {
                operationResponse = (OperationResponse)obj2;
                return true;
            }
            operationResponse = null;
            return false;
        }

        /// <summary>
        /// Tries to parse an encrypted operation response.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="cryptoProvider">The crypto provider.</param>
        /// <param name="operationResponse">The operation response.</param>
        /// <returns> true if the operation response was parsed successfully; otherwise false.</returns>
        public bool TryParseOperationResponseEncrypted(byte[] data, ICryptoProvider cryptoProvider, out OperationResponse operationResponse)
        {
            object obj2;
            if (cryptoProvider == null)
            {
                operationResponse = null;
                return false;
            }
            byte[] buffer = cryptoProvider.Decrypt(data, 2, data.Length - 2);
            if (buffer == null)
            {
                operationResponse = null;
                return false;
            }
            if (operationDataLogger.IsDebugEnabled)
            {
                operationDataLogger.DebugFormat("Decrypted data: data=({0} bytes) {1}", new object[] { buffer.Length, BitConverter.ToString(buffer) });
            }
            int pos = 0;
            if (GpBinaryByteReaderV17.TryReadOperationResponse(buffer, ref pos, out obj2))
            {
                operationResponse = (OperationResponse)obj2;
                return true;
            }
            operationResponse = null;
            return false;
        }

        /// <summary>
        /// Gets the type of the protocol.
        /// </summary>
        /// <value>The type of the protocol.</value>
        public ProtocolType ProtocolType
        {
            get
            {
                return ProtocolType.GpBinaryV17;
            }
        }
    }
}
