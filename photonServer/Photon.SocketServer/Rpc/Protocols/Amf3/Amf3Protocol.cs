using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ExitGames.IO;
using ExitGames.Logging;
using Photon.SocketServer.Rpc.Protocols.GpBinaryByte;
using Photon.SocketServer.Rpc.Reflection;
using Photon.SocketServer.Security;

namespace Photon.SocketServer.Rpc.Protocols.Amf3
{
    /// <summary>
    /// The AMF3 implementation of <see cref="T:Photon.SocketServer.IRpcProtocol"/>.
    /// </summary>
    internal sealed class Amf3Protocol : IRpcProtocol
    {
        /// <summary>
        /// The singleton instance for header v2.
        /// </summary>
        public static readonly Amf3Protocol HeaderV2Instance = new Amf3Protocol(ProtocolType.Amf3V152, RtsMessageHeaderConverterAmf3V2.Instance);

        /// <summary>
        /// Version 1.6.
        /// </summary>
        public static readonly Version ProtocolVersion = new Version(1, 6);

        /// <summary>
        /// the rts message header size
        /// </summary>
        private readonly byte headerSize;

        /// <summary>
        /// the rts message header writer
        /// </summary>
        private readonly IRtsMessageHeaderConverter headerWriter;
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        ///  The protocol type
        /// </summary>
        private readonly ProtocolType protocolType;


        /// <summary>
        ///  Initializes a new instance of the <see cref="T:Photon.SocketServer.Rpc.Protocols.Amf3.Amf3Protocol"/> class.
        /// </summary>
        /// <param name="protocolType">The protocol Type.</param>
        /// <param name="headerWriter">The header Provider.</param>
        private Amf3Protocol(ProtocolType protocolType, IRtsMessageHeaderConverter headerWriter)
        {
            this.protocolType = protocolType;
            this.headerWriter = headerWriter;
            this.headerSize = headerWriter.HeaderSize;
        }

        /// <summary>
        /// Serialze an object to a stream
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="obj">The object to serialize.</param>
        public void Serialize(Stream stream, object obj)
        {
            new Amf3Writer(stream).Write(obj);
        }

        /// <summary>
        /// The serialize event.
        /// </summary>
        /// <param name="eventData">The event data.</param>
        /// <returns> The serialized event</returns>
        public byte[] SerializeEventData(EventData eventData)
        {
            using (ReusableMemoryStream stream = new ReusableMemoryStream())
            {
                BigEndianBinaryWriter binaryWriter = new BigEndianBinaryWriter(stream);
                stream.Position = this.headerSize;
                stream.WriteByte(eventData.Code);
                if ((eventData.Parameters == null) || (eventData.Parameters.Count == 0))
                {
                    binaryWriter.WriteInt16(0);
                }
                else
                {
                    binaryWriter.WriteInt16((short)eventData.Parameters.Count);
                    Amf3Writer writer2 = new Amf3Writer(binaryWriter);
                    foreach (KeyValuePair<byte, object> pair in eventData.Parameters)
                    {
                        writer2.WriteInteger((long)((ulong)pair.Key));
                        writer2.Write(pair.Value);
                    }
                }
                stream.Position = 0L;
                this.headerWriter.WriteHeader(stream, RtsMessageType.Event, false);
                return stream.ToArray();
            }
        }

        /// <summary>
        /// Not supported by the AMF3 protocol.
        /// </summary>
        /// <param name="eventData">The event data.</param>
        /// <param name="cryptoProvider"> The crypto provider.</param>
        /// <returns>Throws a <see cref="T:System.NotSupportedException"/>.</returns>
        /// <remarks>Not supported by AMF3.</remarks>
        public byte[] SerializeEventDataEncrypted(IEventData eventData, ICryptoProvider cryptoProvider)
        {
            throw new NotSupportedException("Encrypted events are not supported by the AMF3 protocol.");
        }

        /// <summary>
        /// The serialize init request.
        /// </summary>
        /// <param name="applicationId"> The application id.</param>
        /// <param name="clientVersion">The client version.</param>
        /// <returns>the serialized init request</returns>
        public byte[] SerializeInitRequest(string applicationId, Version clientVersion)
        {
            return Protocol.SerializeInitRequest(this.headerWriter, ProtocolVersion, clientVersion, applicationId);
        }

        /// <summary>
        /// The serialize init response.
        /// </summary>
        /// <returns>dummy bytes</returns>
        public byte[] SerializeInitResponse()
        {
            return Protocol.SerializeInitResponse(this.headerWriter);
        }

        /// <summary>
        /// Not supported by the AMF3 protocol..
        /// </summary>
        /// <param name="operationRequest">The operation request.</param>
        /// <returns>Throws a <see cref="T:System.NotSupportedException"/>.</returns>
        ///          <exception cref="T:System.NotSupportedException">
        ///  Not supported by AMF3.
        /// </exception>
        public byte[] SerializeInternalOperationRequest(OperationRequest operationRequest)
        {
            throw new NotSupportedException("Internal operations are not supported by the Amf3 protocol.");
        }

        /// <summary>
        /// Not supported by the AMF3 protocol.
        /// </summary>
        /// <param name="operationResponse">The operation response to serialize.</param>
        /// <returns>Throws a <see cref="T:System.NotSupportedException"/>.</returns>
        /// <exception cref="T:System.NotSupportedException">
        /// Not supported by AMF3.
        ///</exception>
        public byte[] SerializeInternalOperationResponse(OperationResponse operationResponse)
        {
            throw new NotSupportedException("Internal operations are not supported by the Amf3 protocol.");
        }

        /// <summary>
        /// Serializes an <see cref="T:Photon.SocketServer.OperationRequest"/>.
        /// </summary>
        /// <param name="operationRequest">The operation request.</param>
        /// <returns>The serialized operation request.</returns>
        public byte[] SerializeOperationRequest(OperationRequest operationRequest)
        {
            using (ReusableMemoryStream stream = new ReusableMemoryStream())
            {
                IBinaryWriter binaryWriter = new BigEndianBinaryWriter(stream);
                stream.Position = this.headerSize;
                binaryWriter.WriteByte(operationRequest.OperationCode);
                binaryWriter.WriteInt16((short)operationRequest.Parameters.Count);
                Amf3Writer writer2 = new Amf3Writer(binaryWriter);
                foreach (KeyValuePair<byte, object> pair in operationRequest.Parameters)
                {
                    writer2.WriteInteger((long)((ulong)pair.Key));
                    writer2.Write(pair.Value);
                }
                stream.Position = 0L;
                this.headerWriter.WriteHeader(stream, RtsMessageType.Operation, false);
                return stream.ToArray();
            }
        }

        /// <summary>
        /// Not supported by the AMF3 protocol.
        /// </summary>
        /// <param name="operationRequest">The operation request.</param>
        /// <param name="cryptoProvider"> An <see cref="T:Photon.SocketServer.Security.ICryptoProvider"/> instance used to encrypt the operation request.</param>
        /// <returns>Throws a <see cref="T:System.NotSupportedException"/>.</returns>
        /// <exception cref="T:System.NotSupportedException">
        /// Not supported by AMF3.
        ///</exception>
        public byte[] SerializeOperationRequestEncrypted(OperationRequest operationRequest, ICryptoProvider cryptoProvider)
        {
            throw new NotSupportedException("Encrypted operations are not supported by the AMF3 protocol.");
        }

        /// <summary>
        /// Serializes an <see cref="T:Photon.SocketServer.OperationResponse"/>.
        /// </summary>
        /// <param name="operationResponse">The operation response to serialize.</param>
        /// <returns>The serialized operation response.</returns>
        public byte[] SerializeOperationResponse(OperationResponse operationResponse)
        {
            using (ReusableMemoryStream stream = new ReusableMemoryStream())
            {
                BigEndianBinaryWriter binaryWriter = new BigEndianBinaryWriter(stream);
                stream.Position = this.headerSize;
                binaryWriter.WriteByte(operationResponse.OperationCode);
                binaryWriter.WriteInt16(operationResponse.ReturnCode);
                if (string.IsNullOrEmpty(operationResponse.DebugMessage))
                {
                    binaryWriter.WriteByte(0x2a);
                }
                else
                {
                    binaryWriter.WriteByte(0x73);
                    binaryWriter.WriteUTF(operationResponse.DebugMessage);
                }
                if ((operationResponse.Parameters == null) || (operationResponse.Parameters.Count == 0))
                {
                    binaryWriter.WriteInt16(0);
                }
                else
                {
                    binaryWriter.WriteInt16((short)operationResponse.Parameters.Count);
                    Amf3Writer writer2 = new Amf3Writer(binaryWriter);
                    foreach (KeyValuePair<byte, object> pair in operationResponse.Parameters)
                    {
                        writer2.WriteInteger((long)((ulong)pair.Key));
                        writer2.Write(pair.Value);
                    }
                }
                stream.Position = 0L;
                this.headerWriter.WriteHeader(stream, RtsMessageType.OperationResponse, false);
                return stream.ToArray();
            }
        }

        /// <summary>
        /// Not supported by the AMF3 protocol.
        /// </summary>
        /// <param name="operationResponse">The response.</param>
        /// <param name="cryptoProvider">An <see cref="T:Photon.SocketServer.Security.ICryptoProvider"/> instance used to encrypt the operation response.</param>
        /// <returns>Throws a <see cref="T:System.NotSupportedException"/>.</returns>
        /// <exception cref="T:System.NotSupportedException">
        /// Not supported by AMF3.
        ///</exception>
        public byte[] SerializeOperationResponseEncrypted(OperationResponse operationResponse, ICryptoProvider cryptoProvider)
        {
            throw new NotSupportedException("Encrypted operations are not supported by the AMF3 protocol.");
        }

        /// <summary>
        /// The try convert operation parameter.
        /// </summary>
        /// <param name="paramterInfo"> The paramter info.</param>
        /// <param name="value">The value.</param>
        /// <returns>true if operation parameter.</returns>
        public bool TryConvertParameter(ObjectMemberInfo<DataMemberAttribute> paramterInfo, ref object value)
        {
            try
            {
                if (paramterInfo.TypeCode != TypeCode.Object)
                {
                    if (value is int)
                    {
                        switch (paramterInfo.TypeCode)
                        {
                            case TypeCode.Byte:
                                value = (byte)((int)value);
                                return true;

                            case TypeCode.Int16:
                                value = (short)((int)value);
                                return true;

                            case TypeCode.Int32:
                                value = (int)value;
                                return true;

                            case TypeCode.Int64:
                                value = (int)value;
                                return true;

                            case TypeCode.Single:
                                value = (int)value;
                                return true;

                            case TypeCode.Double:
                                value = (int)value;
                                return true;
                        }
                    }
                    else if (value is double)
                    {
                        switch (paramterInfo.TypeCode)
                        {
                            case TypeCode.Byte:
                                value = (byte)((double)value);
                                return true;

                            case TypeCode.Int16:
                                value = (short)((double)value);
                                return true;

                            case TypeCode.Int32:
                                value = (int)((double)value);
                                return true;

                            case TypeCode.Int64:
                                value = (long)((double)value);
                                return true;

                            case TypeCode.Single:
                                value = (float)((double)value);
                                return true;

                            case TypeCode.Double:
                                value = (double)value;
                                return true;
                        }
                    }
                }
                else
                {
                    if (paramterInfo.ValueType.IsAssignableFrom(typeof(Guid)) && (paramterInfo.ValueType != typeof(object)))
                    {
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
                    if (paramterInfo.ValueType == typeof(double[]))
                    {
                        object[] objArray = value as object[];
                        if (objArray == null)
                        {
                            return false;
                        }
                        double[] numArray = new double[objArray.Length];
                        for (int i = 0; i < objArray.Length; i++)
                        {
                            numArray[i] = Convert.ToDouble(objArray[i]);
                        }
                        value = numArray;
                        return true;
                    }
                    if (paramterInfo.ValueType == typeof(int[]))
                    {
                        object[] objArray2 = value as object[];
                        if (objArray2 == null)
                        {
                            return false;
                        }
                        int[] numArray2 = new int[objArray2.Length];
                        for (int j = 0; j < objArray2.Length; j++)
                        {
                            numArray2[j] = Convert.ToInt32(objArray2[j]);
                        }
                        value = numArray2;
                        return true;
                    }
                    if (paramterInfo.ValueType == typeof(long[]))
                    {
                        object[] objArray3 = value as object[];
                        if (objArray3 == null)
                        {
                            return false;
                        }
                        long[] numArray3 = new long[objArray3.Length];
                        for (int k = 0; k < objArray3.Length; k++)
                        {
                            if (objArray3[k] is double)
                            {
                                numArray3[k] = (long)((double)objArray3[k]);
                            }
                            else
                            {
                                numArray3[k] = (int)objArray3[k];
                            }
                        }
                        value = numArray3;
                        return true;
                    }
                    if (paramterInfo.ValueType == typeof(short[]))
                    {
                        object[] objArray4 = value as object[];
                        if (objArray4 == null)
                        {
                            return false;
                        }
                        short[] numArray4 = new short[objArray4.Length];
                        for (int m = 0; m < objArray4.Length; m++)
                        {
                            numArray4[m] = (short)((int)objArray4[m]);
                        }
                        value = numArray4;
                        return true;
                    }
                    if (paramterInfo.ValueType == typeof(float[]))
                    {
                        object[] objArray5 = value as object[];
                        if (objArray5 == null)
                        {
                            return false;
                        }
                        float[] numArray5 = new float[objArray5.Length];
                        for (int n = 0; n < objArray5.Length; n++)
                        {
                            numArray5[n] = Convert.ToSingle(objArray5[n]);
                        }
                        value = numArray5;
                        return true;
                    }
                    if (paramterInfo.ValueType == typeof(string[]))
                    {
                        object[] objArray6 = value as object[];
                        if (objArray6 == null)
                        {
                            return false;
                        }
                        string[] strArray = new string[objArray6.Length];
                        for (int num6 = 0; num6 < objArray6.Length; num6++)
                        {
                            strArray[num6] = (string)objArray6[num6];
                        }
                        value = strArray;
                        return true;
                    }
                    if (paramterInfo.ValueType == typeof(Hashtable[]))
                    {
                        object[] objArray7 = value as object[];
                        if (objArray7 == null)
                        {
                            return false;
                        }
                        Hashtable[] hashtableArray = new Hashtable[objArray7.Length];
                        for (int num7 = 0; num7 < objArray7.Length; num7++)
                        {
                            hashtableArray[num7] = (Hashtable)objArray7[num7];
                        }
                        value = hashtableArray;
                        return true;
                    }
                }
                return true;
            }
            catch (Exception exception)
            {
                if (log.IsWarnEnabled)
                {
                    log.Warn(exception);
                }
                return false;
            }
        }

        /// <summary>
        /// Try to parse an object from a stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="obj">The result object.</param>
        /// <returns>True on success.</returns>
        public bool TryParse(Stream stream, out object obj)
        {
            return new Amf3Reader(new BigEndianBinaryReader(stream)).Read(out obj);
        }

        /// <summary>
        /// Converts a byte array to an <see cref="T:Photon.SocketServer.EventData"/> object.
        /// </summary>
        /// <param name="data"> The data.</param>
        /// <param name="eventData">The event data.</param>
        /// <returns>true if successfull.</returns>
        public bool TryParseEventData(byte[] data, out EventData eventData)
        {
            using (MemoryStream stream = new MemoryStream(data, this.headerSize, data.Length - this.headerSize))
            {
                BigEndianBinaryReader binaryReader = new BigEndianBinaryReader(stream);
                byte num = binaryReader.ReadByte();
                short capacity = binaryReader.ReadInt16();
                Dictionary<byte, object> dictionary = new Dictionary<byte, object>(capacity);
                if (capacity > 0)
                {
                    Amf3Reader reader2 = new Amf3Reader(binaryReader);
                    for (short i = 0; i < capacity; i = (short)(i + 1))
                    {
                        object obj2;
                        object obj3;
                        if (!reader2.Read(out obj2))
                        {
                            eventData = null;
                            return false;
                        }
                        byte num4 = (byte)((int)obj2);
                        if (!reader2.Read(out obj3))
                        {
                            eventData = null;
                            return false;
                        }
                        dictionary[num4] = obj3;
                    }
                }
                EventData data2 = new EventData
                {
                    Code = num,
                    Parameters = dictionary
                };
                eventData = data2;
                return true;
            }
        }

        /// <summary>
        /// Not supported by the AMF3 protocol.
        /// </summary>
        /// <param name="data"> The data.</param>
        /// <param name="cryptoProvider"> The crypto Provider.</param>
        /// <param name="eventData">The event data.</param>
        /// <returns>Always false.</returns>
        public bool TryParseEventDataEncrypted(byte[] data, ICryptoProvider cryptoProvider, out EventData eventData)
        {
            eventData = null;
            return false;
        }

        /// <summary>
        ///  Tries to parse the message header.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="header">The header.</param>
        /// <returns>True on success.</returns>
        public bool TryParseMessageHeader(byte[] data, out RtsMessageHeader header)
        {
            return this.headerWriter.TryParseHeader(data, out header);
        }

        /// <summary>
        ///  Tries to parse an <see cref="T:Photon.SocketServer.OperationRequest"/>.
        /// </summary>
        /// <param name="data"> The data.</param>
        /// <param name="operationRequest">The operation request.</param>
        /// <returns>True if request was parsed successfully.</returns>
        public bool TryParseOperationRequest(byte[] data, out OperationRequest operationRequest)
        {
            using (MemoryStream stream = new MemoryStream(data, this.headerSize, data.Length - this.headerSize))
            {
                BigEndianBinaryReader binaryReader = new BigEndianBinaryReader(stream);
                byte num = binaryReader.ReadByte();
                short capacity = binaryReader.ReadInt16();
                OperationRequest request = new OperationRequest
                {
                    OperationCode = num,
                    Parameters = new Dictionary<byte, object>(capacity)
                };
                operationRequest = request;
                if (capacity > 0)
                {
                    Amf3Reader reader2 = new Amf3Reader(binaryReader);
                    for (short i = 0; i < capacity; i = (short)(i + 1))
                    {
                        object obj2;
                        if (!reader2.Read(out obj2))
                        {
                            operationRequest = null;
                            return false;
                        }
                        byte num4 = (byte)((int)obj2);
                        if (!reader2.Read(out obj2))
                        {
                            operationRequest = null;
                            return false;
                        }
                        operationRequest.Parameters[num4] = obj2;
                    }
                }
                return true;
            }
        }

        /// <summary>
        ///  Not supported by the AMF3 protocol.
        /// </summary>
        /// <param name="data"> The data.</param>
        /// <param name="cryptoProvider">An <see cref="T:Photon.SocketServer.Security.ICryptoProvider"/> instance used to decrypt encrypted operation requests.</param>
        /// <param name="operationRequest">The operation request.</param>
        /// <returns>Always false.</returns>
        public bool TryParseOperationRequestEncrypted(byte[] data, ICryptoProvider cryptoProvider, out OperationRequest operationRequest)
        {
            operationRequest = null;
            return false;
        }

        /// <summary>
        /// Tries to parse an operation response.
        /// </summary>
        /// <param name="data"> A byte array containing the binary operation response data.</param>
        /// <param name="operationResponse">Contains the parsed operation response, if the methods returns with success;
        /// otherwise, the parameter will be uninitialized. </param>
        /// <returns>true if the operation response was parsed successfully; otherwise false.</returns>
        public bool TryParseOperationResponse(byte[] data, out OperationResponse operationResponse)
        {
            using (MemoryStream stream = new MemoryStream(data, this.headerSize, data.Length - this.headerSize))
            {
                string str;
                BigEndianBinaryReader reader = new BigEndianBinaryReader(stream);
                byte num = reader.ReadByte();
                short num2 = reader.ReadInt16();
                switch (((GpType)reader.ReadByte()))
                {
                    case GpType.Null:
                        str = null;
                        break;

                    case GpType.String:
                        if (GpBinaryByteReader.ReadString(reader, out str))
                        {
                            break;
                        }
                        operationResponse = null;
                        return false;

                    default:
                        operationResponse = null;
                        return false;
                }
                short capacity = reader.ReadInt16();
                Dictionary<byte, object> dictionary = new Dictionary<byte, object>(capacity);
                if (capacity > 0)
                {
                    Amf3Reader reader2 = new Amf3Reader(reader);
                    for (short i = 0; i < capacity; i = (short)(i + 1))
                    {
                        object obj2;
                        object obj3;
                        if (!reader2.Read(out obj2))
                        {
                            operationResponse = null;
                            return false;
                        }
                        byte num5 = (byte)((int)obj2);
                        if (!reader2.Read(out obj3))
                        {
                            operationResponse = null;
                            return false;
                        }
                        dictionary[num5] = obj3;
                    }
                }
                OperationResponse response = new OperationResponse
                {
                    DebugMessage = str,
                    ReturnCode = num2,
                    OperationCode = num,
                    Parameters = dictionary
                };
                operationResponse = response;
                return true;
            }
        }

        /// <summary>
        /// Not supported by the AMF3 protocol.
        /// </summary>
        /// <param name="data"> A byte array containing the binary operation response data.</param>
        /// <param name="cryptoProvider">A <see cref="T:Photon.SocketServer.Security.ICryptoProvider"/> instance used to decrpyt an encrypted operation response.</param>
        /// <param name="operationResponse">Contains the parsed operation response, if the methods returns with success;
        /// otherwise, the parameter will be uninitialized. </param>
        /// <returns>Always false.</returns>
        public bool TryParseOperationResponseEncrypted(byte[] data, ICryptoProvider cryptoProvider, out OperationResponse operationResponse)
        {
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
                return this.protocolType;
            }
        }
    }
}
