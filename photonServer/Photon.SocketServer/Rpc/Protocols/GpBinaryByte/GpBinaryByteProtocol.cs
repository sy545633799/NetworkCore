using System;
using System.Collections.Generic;
using System.IO;
using ExitGames.IO;
using ExitGames.Logging;
using Photon.SocketServer.Rpc.Reflection;
using Photon.SocketServer.Security;

namespace Photon.SocketServer.Rpc.Protocols.GpBinaryByte
{
    /// <summary>
    /// The GpBinary implementation of <see cref="T:Photon.SocketServer.IRpcProtocol"/>.
    /// </summary>
    internal sealed class GpBinaryByteProtocol : IRpcProtocol
    {
        /// <summary>
        /// The parameter code debug.
        /// </summary>
        internal const byte ParameterCodeDebug = 1;

        /// <summary>
        ///  The parameter code error.
        /// </summary>
        internal const byte ParameterCodeError = 0;

        /// <summary>
        /// The parameter code event.
        /// </summary>
        internal const byte ParameterCodeEvent = 0xf4;

        /// <summary>
        ///  The parameter code operation.
        /// </summary>
        internal const byte ParameterCodeOperation = 0xf4;

        /// <summary>
        /// The protocol instane with the new header.
        /// </summary>
        public static readonly GpBinaryByteProtocol HeaderV2Instance = new GpBinaryByteProtocol(ProtocolType.GpBinaryV152, RtsMessageHeaderConverterBinaryV2.Instance);

        /// <summary>

        /// The protocol version 1.5.
        /// </summary>
        public static readonly Version ProtocolVersion = new Version(1, 5);

        /// <summary>
        /// An <see cref="T:ExitGames.Logging.ILogger"/> instance used to log messages to the logging framework.
        /// </summary>
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        ///  An <see cref="T:ExitGames.Logging.ILogger"/> instance used to log operations to the logging framework.
        /// </summary>
        private static readonly ILogger operationDataLogger = LogManager.GetLogger("OperationData");

        /// <summary>
        /// the rts message header size
        /// </summary>
        private readonly byte headerSize;

        /// <summary>
        /// the rts message header writer
        /// </summary>
        private readonly IRtsMessageHeaderConverter headerWriter;

        /// <summary>
        /// The protocol type
        /// </summary>
        private readonly ProtocolType protocolType;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Photon.SocketServer.Rpc.Protocols.GpBinaryByte.GpBinaryByteProtocol"/> class.
        /// </summary>
        /// <param name="protocolType">The protocol Type.</param>
        /// <param name="headerWriter">The header Provider.</param>
        private GpBinaryByteProtocol(ProtocolType protocolType, IRtsMessageHeaderConverter headerWriter)
        {
            this.protocolType = protocolType;
            this.headerWriter = headerWriter;
            this.headerSize = headerWriter.HeaderSize;
        }

        /// <summary>
        ///  Serialze an object to a stream
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="obj">The object to serialize.</param>
        public void Serialize(Stream stream, object obj)
        {
            BigEndianBinaryWriter writer = new BigEndianBinaryWriter(stream);
            GpBinaryByteWriter.Write(writer, obj);
        }

        /// <summary>
        /// The serialize event.
        /// </summary>
        /// <param name="eventData"> The event data.</param>
        /// <returns>The serialized event.</returns>
        /// <exception cref="T:System.IO.InvalidDataException">
        /// A value can not be serialized.
        ///</exception>
        ///<exception cref="T:System.ArrayTypeMismatchException">
        /// A collection with different types can not be serialized.
        ///</exception>
        public byte[] SerializeEventData(EventData eventData)
        {
            using (ReusableMemoryStream stream = new ReusableMemoryStream())
            {
                this.SerializeEventData(stream, eventData);
                stream.Position = 0L;
                this.headerWriter.WriteHeader(stream, RtsMessageType.Event, false);
                return stream.ToArray();
            }
        }

        /// <summary>
        /// serializes the event data to a stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="eventData">The event data.</param>
        private void SerializeEventData(ReusableMemoryStream stream, IEventData eventData)
        {
            if ((eventData.Parameters != null) && eventData.Parameters.Remove(0xf4))
            {
                log.WarnFormat("SendEvent - removed reserved parameter {1} from event {0}", new object[] { eventData.Code, (byte)0xf4 });
            }
            BigEndianBinaryWriter writer = new BigEndianBinaryWriter(stream);
            stream.Seek((long)this.headerSize, SeekOrigin.Begin);
            bool flag = eventData.Parameters != null;
            int num = flag ? (eventData.Parameters.Count + 1) : 1;
            writer.WriteInt16((short)num);
            writer.WriteByte(0xf4);
            GpBinaryByteWriter.Write(writer, eventData.Code);
            if (flag)
            {
                foreach (KeyValuePair<byte, object> pair in eventData.Parameters)
                {
                    writer.WriteByte(pair.Key);
                    GpBinaryByteWriter.Write(writer, pair.Value);
                }
            }
        }

        /// <summary>
        /// Encrypts an event.
        /// </summary>
        /// <param name="eventData">The event data.</param>
        /// <param name="cryptoProvider"> The crypto provider.</param>
        /// <returns>The encrypted event.</returns>
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
                this.SerializeEventData(stream, eventData);
                byte[] data = stream.ToArray();
                buffer = cryptoProvider.Encrypt(data, 0, data.Length);
            }
            using (ReusableMemoryStream stream2 = new ReusableMemoryStream())
            {
                stream2.Position = this.headerSize;
                stream2.Write(buffer, 0, buffer.Length);
                stream2.Position = 0L;
                this.headerWriter.WriteHeader(stream2, RtsMessageType.Operation, true);
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
            return Protocol.SerializeInitRequest(this.headerWriter, ProtocolVersion, clientVersion, applicationId);
        }

        /// <summary>
        /// The serialize init response.
        /// </summary>
        /// <returns>The serialized init response.</returns>
        public byte[] SerializeInitResponse()
        {
            return Protocol.SerializeInitResponse(this.headerWriter);
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
        /// <param name="operationResponse"> The operation response.</param>
        /// <returns>The serialized operation response.</returns>
        public byte[] SerializeInternalOperationResponse(OperationResponse operationResponse)
        {
            return this.SerializeOperationResponse(operationResponse, RtsMessageType.InternalOperationResponse);
        }

        /// <summary>
        /// Serializes an operation request.
        /// </summary>
        /// <param name="operationRequest"> The operation request.</param>
        /// <returns>a serialized operation request message</returns>
        public byte[] SerializeOperationRequest(OperationRequest operationRequest)
        {
            using (ReusableMemoryStream stream = new ReusableMemoryStream())
            {
                stream.Position = this.headerSize;
                SerializeOperationRequest(stream, operationRequest);
                stream.Position = 0L;
                this.headerWriter.WriteHeader(stream, RtsMessageType.Operation, false);
                return stream.ToArray();
            }
        }

        /// <summary>
        /// Serializes an <see cref="T:Photon.SocketServer.OperationResponse"/>.
        /// </summary>
        /// <param name="operationRequest">The operation request to serialize.</param>
        /// <param name="messageType">
        /// The message type. 
        /// Should be eiter <see cref="F:Photon.SocketServer.Rpc.Protocols.RtsMessageType.OperationResponse"/> or <see cref="F:Photon.SocketServer.Rpc.Protocols.RtsMessageType.InternalOperationResponse"/>.
        /// </param>
        /// <returns>A serialized operation response.</returns>
        private byte[] SerializeOperationRequest(OperationRequest operationRequest, RtsMessageType messageType)
        {
            using (ReusableMemoryStream stream = new ReusableMemoryStream())
            {
                stream.Position = this.headerSize;
                SerializeOperationRequest(stream, operationRequest);
                stream.Position = 0L;
                this.headerWriter.WriteHeader(stream, messageType, false);
                return stream.ToArray();
            }
        }

        /// <summary>
        ///  Serializes an operation request.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="operationRequest"> The operation request.</param>
        /// <exception cref="T:System.IO.InvalidDataException">
        /// A value can not be serialized.
        ///</exception>
        ///<exception cref="T:System.ArrayTypeMismatchException">
        ///  A collection with different types can not be serialized.
        ///</exception>
        private static void SerializeOperationRequest(Stream stream, OperationRequest operationRequest)
        {
            IBinaryWriter writer = new BigEndianBinaryWriter(stream);
            writer.WriteByte(operationRequest.OperationCode);
            writer.WriteBoolean(false);
            if (operationRequest.Parameters != null)
            {
                writer.WriteInt16((short)operationRequest.Parameters.Count);
                foreach (KeyValuePair<byte, object> pair in operationRequest.Parameters)
                {
                    writer.WriteByte(pair.Key);
                    GpBinaryByteWriter.Write(writer, pair.Value);
                }
            }
            else
            {
                writer.WriteInt16(0);
            }
        }

        /// <summary>
        /// The serialize operation request.
        /// </summary>
        /// <param name="operationRequest"> The operation request.</param>
        /// <param name="cryptoProvider">The <see cref="T:System.Security.Cryptography.ICryptoTransform"/> used to encrypt operation response data.</param>
        /// <returns>a serialized operation request message</returns>
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
                stream2.Position = this.headerSize;
                stream2.Write(buffer, 0, buffer.Length);
                stream2.Position = 0L;
                this.headerWriter.WriteHeader(stream2, RtsMessageType.Operation, true);
                return stream2.ToArray();
            }
        }

        /// <summary>
        /// Serializes an <see cref="T:Photon.SocketServer.OperationResponse"/>.
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
        /// The message type. 
        ///Should be eiter <see cref="F:Photon.SocketServer.Rpc.Protocols.RtsMessageType.OperationResponse"/> or <see cref="F:Photon.SocketServer.Rpc.Protocols.RtsMessageType.InternalOperationResponse"/>.
        /// </param>
        /// <returns>The serialized operation response.</returns>
        private byte[] SerializeOperationResponse(OperationResponse operationResponse, RtsMessageType messageType)
        {
            using (ReusableMemoryStream stream = new ReusableMemoryStream())
            {
                stream.Seek((long)this.headerSize, SeekOrigin.Begin);
                SerializeOperationResponse(stream, operationResponse);
                stream.Position = 0L;
                this.headerWriter.WriteHeader(stream, messageType, false);
                return stream.ToArray();
            }
        }

        /// <summary>
        /// Serializes an operation response.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="operationResponse"> The operation response.</param>
        /// <exception cref="T:System.IO.InvalidDataException">
        /// A value can not be serialized.
        ///</exception>
        ///<exception cref="T:System.ArrayTypeMismatchException">
        ///  A collection with different types can not be serialized.
        ///</exception>
        private static void SerializeOperationResponse(Stream stream, OperationResponse operationResponse)
        {
            if (operationResponse.Parameters != null)
            {
                if (operationResponse.Parameters.Remove(0xf4))
                {
                    log.WarnFormat("SendOperationResponse - removed reserved parameter {1} from operation response {0}", new object[] { operationResponse.OperationCode, (byte)0xf4 });
                }
                if (operationResponse.Parameters.Remove(0))
                {
                    log.WarnFormat("SendOperationResponse - removed reserved parameter {1} from operation response {0}", new object[] { operationResponse.OperationCode, (byte)0 });
                }
                if (operationResponse.Parameters.Remove(1))
                {
                    log.WarnFormat("SendOperationResponse - removed reserved parameter {1} from operation response {0}", new object[] { operationResponse.OperationCode, (byte)1 });
                }
            }
            IBinaryWriter writer = new BigEndianBinaryWriter(stream);
            bool flag = !string.IsNullOrEmpty(operationResponse.DebugMessage);
            bool flag2 = operationResponse.Parameters != null;
            int num = flag2 ? operationResponse.Parameters.Count : 0;
            num += flag ? 3 : 2;
            writer.WriteInt16((short)num);
            writer.WriteByte(0xf4);
            GpBinaryByteWriter.Write(writer, operationResponse.OperationCode);
            writer.WriteByte(0);
            GpBinaryByteWriter.Write(writer, (int)operationResponse.ReturnCode);
            if (flag)
            {
                writer.WriteByte(1);
                GpBinaryByteWriter.Write(writer, operationResponse.DebugMessage);
            }
            if (flag2)
            {
                foreach (KeyValuePair<byte, object> pair in operationResponse.Parameters)
                {
                    writer.WriteByte(pair.Key);
                    GpBinaryByteWriter.Write(writer, pair.Value);
                }
            }
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
            using (ReusableMemoryStream stream = new ReusableMemoryStream())
            {
                SerializeOperationResponse(stream, operationResponse);
                byte[] data = stream.ToArray();
                buffer = cryptoProvider.Encrypt(data);
            }
            using (ReusableMemoryStream stream2 = new ReusableMemoryStream())
            {
                stream2.Position = this.headerSize;
                stream2.Write(buffer, 0, buffer.Length);
                stream2.Position = 0L;
                this.headerWriter.WriteHeader(stream2, RtsMessageType.OperationResponse, true);
                return stream2.ToArray();
            }
        }

        /// <summary>
        /// The convert operation parameter.
        /// </summary>
        /// <param name="paramterInfo">The paramter info.</param>
        /// <param name="value">The value.</param>
        /// <returns><paramref name = "value" /> or a Guid if value is 16 bytes.</returns>
        public bool TryConvertParameter(ObjectMemberInfo<DataMemberAttribute> paramterInfo, ref object value)
        {
            if (!paramterInfo.ValueType.IsAssignableFrom(typeof(Guid)) || (paramterInfo.ValueType == typeof(object)))
            {
                return true;
            }
            if (value is Guid)
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
        /// <param name="stream">The stream.</param>
        /// <param name="obj">The result object.</param>
        /// <returns>True on success.</returns>
        public bool TryParse(Stream stream, out object obj)
        {
            BigEndianBinaryReader binaryReader = new BigEndianBinaryReader(stream);
            return GpBinaryByteReader.Read(binaryReader, out obj);
        }

        /// <summary>
        /// The try parse event data.
        /// </summary>
        /// <param name="data"> The data.</param>
        /// <param name="eventData">The event data.</param>
        /// <returns>true if successful.</returns>
        public bool TryParseEventData(byte[] data, out EventData eventData)
        {
            using (MemoryStream stream = new MemoryStream(data, this.headerSize, data.Length - this.headerSize))
            {
                return this.TryParseEventData(stream, out eventData);
            }
        }        /// <summary>

        /// The try parse event data.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="eventData">The event data.</param>
        /// <returns>true on success.</returns>
        internal bool TryParseEventData(Stream stream, out EventData eventData)
        {
            object obj3;
            BigEndianBinaryReader binaryReader = new BigEndianBinaryReader(stream);
            short capacity = binaryReader.ReadInt16();
            Dictionary<byte, object> dictionary = new Dictionary<byte, object>(capacity);
            for (short i = 0; i < capacity; i = (short)(i + 1))
            {
                object obj2;
                byte key = binaryReader.ReadByte();
                if (GpBinaryByteReader.Read(binaryReader, out obj2))
                {
                    dictionary.Add(key, obj2);
                }
                else
                {
                    eventData = null;
                    return false;
                }
            }
            EventData data = new EventData
            {
                Parameters = dictionary
            };
            eventData = data;
            if (dictionary.TryGetValue(0xf4, out obj3) && (obj3 is byte))
            {
                eventData.Code = (byte)obj3;
            }
            return true;
        }

        /// <summary>
        ///  The try parse encrypted event data.
        /// </summary>
        /// <param name="data"> The data.</param>
        /// <param name="cryptoProvider">The crypto provider.</param>
        /// <param name="eventData">The event data.</param>
        /// <returns> true if successful.</returns>
        public bool TryParseEventDataEncrypted(byte[] data, ICryptoProvider cryptoProvider, out EventData eventData)
        {
            if (cryptoProvider == null)
            {
                eventData = null;
                return false;
            }
            byte[] buffer = cryptoProvider.Decrypt(data, this.headerSize, data.Length - this.headerSize);
            using (MemoryStream stream = new MemoryStream(buffer))
            {
                if (operationDataLogger.IsDebugEnabled)
                {
                    operationDataLogger.DebugFormat("Decrypted data: data=({0} bytes) {1}", new object[] { buffer.Length, BitConverter.ToString(buffer) });
                }
                return this.TryParseEventData(stream, out eventData);
            }
        }

        /// <summary>
        /// Tries to parse the message header.
        /// </summary>
        /// <param name="data"> The data.</param>
        /// <param name="header"> The header.</param>
        /// <returns>  True on success.</returns>
        public bool TryParseMessageHeader(byte[] data, out RtsMessageHeader header)
        {
            return this.headerWriter.TryParseHeader(data, out header);
        }

        /// <summary>
        /// Tries to parse an <see cref="T:Photon.SocketServer.OperationRequest"/>.
        /// </summary>
        /// <param name="data">The raw request data.</param>
        /// <param name="operationRequest"> The operation request.</param>
        /// <returns>True if request was parsed successfully.</returns>
        public bool TryParseOperationRequest(byte[] data, out OperationRequest operationRequest)
        {
            using (MemoryStream stream = new MemoryStream(data, this.headerSize, data.Length - this.headerSize))
            {
                return TryParseOperationRequest(stream, out operationRequest);
            }
        }

        /// <summary>
        ///  Tries to parse an operation request.
        /// </summary>
        /// <param name="stream"> The stream.</param>
        /// <param name="operationRequest">The operation request.</param>
        /// <returns>True on success.</returns>
        private static bool TryParseOperationRequest(MemoryStream stream, out OperationRequest operationRequest)
        {
            BigEndianBinaryReader binaryReader = new BigEndianBinaryReader(stream);
            byte num = binaryReader.ReadByte();
            binaryReader.ReadBoolean();
            short capacity = binaryReader.ReadInt16();
            if ((capacity < 0) || (capacity > ((stream.Length - stream.Position) / 2L)))
            {
                if (log.IsDebugEnabled)
                {
                    log.DebugFormat("Invalid parameter count: count={0}, bytesLeft={1}", new object[] { capacity, stream.Length - stream.Position });
                }
                operationRequest = null;
                return false;
            }
            OperationRequest request = new OperationRequest
            {
                Parameters = new Dictionary<byte, object>(capacity),
                OperationCode = num
            };
            operationRequest = request;
            for (short i = 0; i < capacity; i = (short)(i + 1))
            {
                object obj2;
                byte key = binaryReader.ReadByte();
                if (GpBinaryByteReader.Read(binaryReader, out obj2))
                {
                    operationRequest.Parameters.Add(key, obj2);
                }
                else
                {
                    operationRequest = null;
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        ///  Tries to parse an <see cref="T:Photon.SocketServer.OperationRequest"/>.
        /// </summary>
        /// <param name="data"> The raw request data.</param>
        /// <param name="cryptoProvider">An <see cref="T:Photon.SocketServer.Security.ICryptoProvider"/> instance used to decrypt encrypted operation requests.</param>
        /// <param name="operationRequest">The operation request.</param>
        /// <returns>True if request was parsed successfully.</returns>
        public bool TryParseOperationRequestEncrypted(byte[] data, ICryptoProvider cryptoProvider, out OperationRequest operationRequest)
        {
            if (cryptoProvider == null)
            {
                operationRequest = null;
                return false;
            }
            byte[] buffer = cryptoProvider.Decrypt(data, this.headerSize, data.Length - this.headerSize);
            if (buffer == null)
            {
                operationRequest = null;
                return false;
            }
            using (MemoryStream stream = new MemoryStream(buffer))
            {
                if (operationDataLogger.IsDebugEnabled)
                {
                    operationDataLogger.DebugFormat("Decrypted data: data=({0} bytes) {1}", new object[] { buffer.Length, BitConverter.ToString(buffer) });
                }
                return TryParseOperationRequest(stream, out operationRequest);
            }
        }

        /// <summary>
        ///  Tries to parse an operation response.
        /// </summary>
        /// <param name="data">A byte array containing the binary operation response data.</param>
        /// <param name="operationResponse">Contains the parsed operation response, if the methods returns with success;
        /// otherwise, the parameter will be uninitialized. </param>
        /// <returns>true if the operation response was parsed successfully; otherwise false.</returns>
        public bool TryParseOperationResponse(byte[] data, out OperationResponse operationResponse)
        {
            using (MemoryStream stream = new MemoryStream(data, this.headerSize, data.Length - this.headerSize))
            {
                return this.TryParseOperationResponse(stream, out operationResponse);
            }
        }

        /// <summary>
        /// The try parse operation response.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="operationResponse">The operation response.</param>
        /// <returns>true on success.</returns>
        internal bool TryParseOperationResponse(Stream stream, out OperationResponse operationResponse)
        {
            object obj3;
            BigEndianBinaryReader binaryReader = new BigEndianBinaryReader(stream);
            short capacity = binaryReader.ReadInt16();
            Dictionary<byte, object> dictionary = new Dictionary<byte, object>(capacity);
            for (short i = 0; i < capacity; i = (short)(i + 1))
            {
                object obj2;
                byte key = binaryReader.ReadByte();
                if (GpBinaryByteReader.Read(binaryReader, out obj2))
                {
                    dictionary.Add(key, obj2);
                }
                else
                {
                    operationResponse = null;
                    return false;
                }
            }
            byte num4 = 0;
            short num5 = 0;
            string str = null;
            if (dictionary.TryGetValue(0xf4, out obj3) && (obj3 is byte))
            {
                num4 = (byte)obj3;
            }
            if (dictionary.TryGetValue(0, out obj3) && (obj3 is int))
            {
                num5 = (short)((int)obj3);
            }
            if (dictionary.TryGetValue(1, out obj3))
            {
                string str2 = obj3 as string;
                if (str2 != null)
                {
                    str = str2;
                }
            }
            OperationResponse response = new OperationResponse
            {
                DebugMessage = str,
                ReturnCode = num5,
                OperationCode = num4,
                Parameters = dictionary
            };
            operationResponse = response;
            return true;
        }

        /// <summary>
        /// Tries to parse an ecrypted operation response.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="cryptoProvider">   The crypto provider.</param>
        /// <param name="operationResponse">The operation response.</param>
        /// <returns>True on success, otherwise false.</returns>
        public bool TryParseOperationResponseEncrypted(byte[] data, ICryptoProvider cryptoProvider, out OperationResponse operationResponse)
        {
            if (cryptoProvider == null)
            {
                operationResponse = null;
                return false;
            }
            byte[] buffer = cryptoProvider.Decrypt(data, this.headerSize, data.Length - this.headerSize);
            if (buffer == null)
            {
                operationResponse = null;
                return false;
            }
            using (MemoryStream stream = new MemoryStream(buffer))
            {
                if (operationDataLogger.IsDebugEnabled)
                {
                    operationDataLogger.DebugFormat("Decrypted data: data=({0} bytes) {1}", new object[] { buffer.Length, BitConverter.ToString(buffer) });
                }
                return this.TryParseOperationResponse(stream, out operationResponse);
            }
        }

        /// <summary>
        /// Gets the type of the protocol.
        /// </summary>
        /// <value>The type of the protocol.</value>
        public ProtocolType ProtocolType
        {
            get
            {
                return this.protocolType;
            }
        }
    }

}
