using System;
using System.IO;
using ExitGames.IO;
using ExitGames.Logging;
using Photon.SocketServer.Rpc.Reflection;
using Photon.SocketServer.Security;

namespace Photon.SocketServer.Rpc.Protocols.GpBinaryByte
{
    internal class GpBinaryByteProtocolV16 : IRpcProtocol
    {
        // Fields
        private readonly byte headerSize;
        public static readonly GpBinaryByteProtocolV16 HeaderV2Instance = new GpBinaryByteProtocolV16(ProtocolType.GpBinaryV162, RtsMessageHeaderConverterBinaryV2.Instance);
        private readonly IRtsMessageHeaderConverter headerWriter;
        private static readonly ILogger operationDataLogger = LogManager.GetLogger("OperationData");
        private readonly ProtocolType protocolType;
        public static readonly Version ProtocolVersion = new Version(1, 6);

        // Methods
        protected GpBinaryByteProtocolV16(ProtocolType protocolType, IRtsMessageHeaderConverter headerWriter)
        {
            this.protocolType = protocolType;
            this.headerWriter = headerWriter;
            this.headerSize = headerWriter.HeaderSize;
        }

        public void Serialize(Stream stream, object obj)
        {
            BigEndianBinaryWriter writer = new BigEndianBinaryWriter(stream);
            GpBinaryByteWriter.Write(writer, obj);
        }

        public byte[] SerializeEventData(EventData eventData)
        {
            using (ReusableMemoryStream stream = new ReusableMemoryStream())
            {
                stream.Position = this.headerSize;
                SerializeEventData(stream, eventData);
                stream.Position = 0L;
                this.headerWriter.WriteHeader(stream, RtsMessageType.Event, false);
                return stream.ToArray();
            }
        }

        private static void SerializeEventData(ReusableMemoryStream stream, IEventData eventData)
        {
            BigEndianBinaryWriter binaryWriter = new BigEndianBinaryWriter(stream);
            GpBinaryByteWriter.WriteEventData(binaryWriter, eventData);
        }

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
                stream2.Position = this.headerSize;
                stream2.Write(buffer, 0, buffer.Length);
                stream2.Position = 0L;
                this.headerWriter.WriteHeader(stream2, RtsMessageType.Event, true);
                return stream2.ToArray();
            }
        }

        public byte[] SerializeInitRequest(string applicationId, Version clientVersion)
        {
            return Protocol.SerializeInitRequest(this.headerWriter, ProtocolVersion, clientVersion, applicationId);
        }

        public byte[] SerializeInitResponse()
        {
            return Protocol.SerializeInitResponse(this.headerWriter);
        }

        public byte[] SerializeInternalOperationRequest(OperationRequest operationRequest)
        {
            return this.SerializeOperationRequest(operationRequest, RtsMessageType.InternalOperationRequest);
        }

        public byte[] SerializeInternalOperationResponse(OperationResponse operationResponse)
        {
            return this.SerializeOperationResponse(operationResponse, RtsMessageType.InternalOperationResponse);
        }

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

        private static void SerializeOperationRequest(Stream stream, OperationRequest operationRequest)
        {
            IBinaryWriter writer = new BigEndianBinaryWriter(stream);
            GpBinaryByteWriter.WriteOperationRequest(writer, operationRequest);
        }

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

        public byte[] SerializeOperationResponse(OperationResponse operationResponse)
        {
            return this.SerializeOperationResponse(operationResponse, RtsMessageType.OperationResponse);
        }

        private byte[] SerializeOperationResponse(OperationResponse operationResponse, RtsMessageType messageType)
        {
            using (ReusableMemoryStream stream = new ReusableMemoryStream())
            {
                stream.Position = this.headerSize;
                SerializeOperationResponse(stream, operationResponse);
                stream.Position = 0L;
                this.headerWriter.WriteHeader(stream, messageType, false);
                return stream.ToArray();
            }
        }

        private static void SerializeOperationResponse(Stream stream, OperationResponse operationResponse)
        {
            IBinaryWriter writer = new BigEndianBinaryWriter(stream);
            GpBinaryByteWriter.WriteOperationResponse(writer, operationResponse);
        }

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
                stream2.Position = this.headerSize;
                stream2.Write(buffer, 0, buffer.Length);
                stream2.Position = 0L;
                this.headerWriter.WriteHeader(stream2, RtsMessageType.OperationResponse, true);
                return stream2.ToArray();
            }
        }

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

        public bool TryParse(Stream stream, out object obj)
        {
            BigEndianBinaryReader binaryReader = new BigEndianBinaryReader(stream);
            return GpBinaryByteReader.Read(binaryReader, out obj);
        }

        public bool TryParseEventData(byte[] data, out EventData eventData)
        {
            using (MemoryStream stream = new MemoryStream(data, this.headerSize, data.Length - this.headerSize))
            {
                return TryParseEventData(stream, out eventData);
            }
        }

        private static bool TryParseEventData(Stream stream, out EventData eventData)
        {
            object obj2;
            BigEndianBinaryReader binaryReader = new BigEndianBinaryReader(stream);
            if (GpBinaryByteReader.ReadEventData(binaryReader, out obj2))
            {
                eventData = (EventData)obj2;
                return true;
            }
            eventData = null;
            return false;
        }

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
                return TryParseEventData(stream, out eventData);
            }
        }

        public bool TryParseMessageHeader(byte[] data, out RtsMessageHeader header)
        {
            return this.headerWriter.TryParseHeader(data, out header);
        }

        public bool TryParseOperationRequest(byte[] data, out OperationRequest operationRequest)
        {
            using (MemoryStream stream = new MemoryStream(data, this.headerSize, data.Length - this.headerSize))
            {
                return TryParseOperationRequest(stream, out operationRequest);
            }
        }

        private static bool TryParseOperationRequest(MemoryStream stream, out OperationRequest operationRequest)
        {
            object obj2;
            BigEndianBinaryReader binaryReader = new BigEndianBinaryReader(stream);
            if (GpBinaryByteReader.ReadOperationRequest(binaryReader, out obj2))
            {
                operationRequest = (OperationRequest)obj2;
                return true;
            }
            operationRequest = null;
            return false;
        }

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

        public bool TryParseOperationResponse(byte[] data, out OperationResponse operationResponse)
        {
            using (MemoryStream stream = new MemoryStream(data, this.headerSize, data.Length - this.headerSize))
            {
                return TryParseOperationResponse(stream, out operationResponse);
            }
        }

        private static bool TryParseOperationResponse(Stream stream, out OperationResponse operationResponse)
        {
            object obj2;
            BigEndianBinaryReader binaryReader = new BigEndianBinaryReader(stream);
            if (GpBinaryByteReader.ReadOperationResponse(binaryReader, out obj2))
            {
                operationResponse = (OperationResponse)obj2;
                return true;
            }
            operationResponse = null;
            return false;
        }

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
                return TryParseOperationResponse(stream, out operationResponse);
            }
        }

        // Properties
        public ProtocolType ProtocolType
        {
            get
            {
                return this.protocolType;
            }
        }
    }
}
