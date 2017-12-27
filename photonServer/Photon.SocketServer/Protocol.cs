using System;
using System.Collections.Generic;
using System.Text;
using ExitGames.Logging;
using Photon.SocketServer.Rpc.Protocols;
using Photon.SocketServer.Rpc.Protocols.Amf3;
using Photon.SocketServer.Rpc.Protocols.GpBinaryV17;
using Photon.SocketServer.Rpc.Protocols.GpBinaryByte;
using Photon.SocketServer.Rpc.Protocols.Json;

namespace Photon.SocketServer
{
    /// <summary>
    /// This class provides access to the available protocols. 
    /// </summary>
    public static class Protocol
    {
        /// <summary>
        /// The log.
        /// </summary>
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        ///  All existing v1.5 protocols
        /// </summary>
        private static readonly Dictionary<RtsMagicByte, IRpcProtocol> protocols15;

        /// <summary>
        /// All existing v1.6 protocols
        /// </summary>
        private static readonly Dictionary<RtsMagicByte, IRpcProtocol> protocols16;

        internal static readonly int MaxProtocolType;

        // Methods
        static Protocol()
        {
            Dictionary<RtsMagicByte, IRpcProtocol> dictionary = new Dictionary<RtsMagicByte, IRpcProtocol>();
            dictionary.Add(RtsMagicByte.GpBinaryV2, GpBinaryV152);
            protocols15 = dictionary;
            Dictionary<RtsMagicByte, IRpcProtocol> dictionary2 = new Dictionary<RtsMagicByte, IRpcProtocol>();
            dictionary2.Add(RtsMagicByte.GpBinaryV2, GpBinaryV162);
            dictionary2.Add(RtsMagicByte.Amf3V2, Amf3V162);
            protocols16 = dictionary2;
            foreach (object obj2 in Enum.GetValues(typeof(ProtocolType)))
            {
                if (((int)obj2) > MaxProtocolType)
                {
                    MaxProtocolType = (int)obj2;
                }
            }
            MaxProtocolType++;
        }

        /// <summary>
        /// Gets the protocol type for a specified magic number.
        /// </summary>
        /// <param name="magicNumber">The magic number</param>
        /// <returns>The <see cref="T:Photon.SocketServer.Rpc.Protocols.ProtocolType"/> for the specified magic number.</returns>
        internal static bool CheckProtocolType(byte magicNumber)
        {
            return Enum.IsDefined(typeof(RtsMessageType), magicNumber);
        }

        /// <summary>
        /// The serialize init request.
        /// </summary>
        /// <param name="headerWriter">The header Writer.</param>
        /// <param name="protocolVersion">The protocol version</param>
        /// <param name="clientVersion">The client version.</param>
        /// <param name="applicationId">The application id.</param>
        /// <returns>the serialized init request</returns>
        internal static byte[] SerializeInitRequest(IRtsMessageHeaderConverter headerWriter, Version protocolVersion, Version clientVersion, string applicationId)
        {
            using (ReusableMemoryStream stream = new ReusableMemoryStream())
            {
                stream.Position = headerWriter.HeaderSize;
                new RtsInitMessage(new byte[] { (byte)protocolVersion.Major, (byte)protocolVersion.Minor }, new byte[] { (byte)clientVersion.Major, (byte)clientVersion.Minor, (byte)clientVersion.Build }, applicationId).Serialize(stream);
                stream.Position = 0L;
                headerWriter.WriteHeader(stream, RtsMessageType.Init, false);
                return stream.ToArray();
            }
        }

        /// <summary>
        ///  The serialize init response.
        /// </summary>
        /// <param name="headerWriter">The header writer.</param>
        /// <returns> the init response</returns>
        internal static byte[] SerializeInitResponse(IRtsMessageHeaderConverter headerWriter)
        {
            using (ReusableMemoryStream stream = new ReusableMemoryStream())
            {
                stream.Position = headerWriter.HeaderSize;
                RtsInitResponseMessage.Serialize(stream);
                stream.Position = 0L;
                headerWriter.WriteHeader(stream, RtsMessageType.InitResponse, false);
                return stream.ToArray();
            }
        }

        /// <summary>
        /// The try parse init request.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="initRequest">The init request.</param>
        /// <returns>True if init request had correct format.</returns>
        internal static bool TryParseInitRequest(byte[] data, out InitRequest initRequest)
        {
            RtsMessageHeader header;
            RtsInitMessage message;
            if ((data == null) || (data.Length == 0))
            {
                initRequest = null;
                return false;
            }
            RtsMagicByte num = (RtsMagicByte)data[0];
            switch (num)
            {
                case RtsMagicByte.GpBinaryV2:
                    if (RtsMessageHeaderConverterBinaryV2.Instance.TryParseHeader(data, out header))
                    {
                        goto Label_0050;
                    }
                    break;

                case RtsMagicByte.Amf3V2:
                    if (RtsMessageHeaderConverterAmf3V2.Instance.TryParseHeader(data, out header))
                    {
                        goto Label_0050;
                    }
                    break;
            }
            initRequest = null;
            return false;
        Label_0050:
            if (RtsInitMessage.TryParse(data, header.SizeInBytes, out message))
            {
                if (log.IsDebugEnabled)
                {
                    log.DebugFormat("Parsed init message for application {0}, client version {1}.{2}.{3}, protocol {4} version {5}.{6}", new object[] { message.ApplicationId, message.ClientVersion[0], message.ClientVersion[1], message.ClientVersion[2], num, message.ProtocolVersion[0], message.ProtocolVersion[1] });
                }
                Version version = new Version(message.ProtocolVersion[0], message.ProtocolVersion[1]);
                Version clientVersion = new Version(message.ClientVersion[0], message.ClientVersion[1], message.ClientVersion[2]);
                if (version.Major == 1)
                {
                    switch (version.Minor)
                    {
                        case 2:
                            {
                                IRpcProtocol protocol3 = GpBinaryByteProtocolV16Flash.FlashHeaderV2Instance;
                                initRequest = new InitRequest(message.ApplicationId, clientVersion, protocol3);
                                return true;
                            }
                        case 5:
                            IRpcProtocol protocol;
                            if (!protocols15.TryGetValue(header.MagicByte, out protocol))
                            {
                                break;
                            }
                            initRequest = new InitRequest(message.ApplicationId, clientVersion, protocol);
                            return true;

                        case 6:
                            IRpcProtocol protocol2;
                            if (!protocols16.TryGetValue(header.MagicByte, out protocol2))
                            {
                                break;
                            }
                            initRequest = new InitRequest(message.ApplicationId, clientVersion, protocol2);
                            return true;

                        case 7:
                            if (header.MagicByte != RtsMagicByte.GpBinaryV2)
                            {
                                break;
                            }
                            initRequest = new InitRequest(message.ApplicationId, clientVersion, GpBinaryV17);
                            return true;
                    }
                }
            }
            initRequest = null;
            return false;
        }

        internal static bool TryParseWebSocketInitRequest(byte[] data, string applicationName, Version clientVersion, out InitRequest initRequest)
        {
            initRequest = null;
            IRpcProtocol json = Json;
            string str = Encoding.UTF8.GetString(data);
            int index = str.IndexOf("Sec-WebSocket-Protocol: ", StringComparison.OrdinalIgnoreCase);
            if (index > 0)
            {
                int length = str.IndexOfAny(new char[] { '\n', '\r' }, index);
                if (length < 0)
                {
                    length = str.Length;
                }
                string str2 = str.Substring(index, length - index).Replace("Sec-WebSocket-Protocol: ", string.Empty);
                if (!Enum.IsDefined(typeof(ProtocolType), str2))
                {
                    log.WarnFormat("Subprotocol {0} is unknown", new object[] { str2 });
                    return false;
                }
                switch (((ProtocolType)Enum.Parse(typeof(ProtocolType), str2)))
                {
                    case ProtocolType.GpBinaryV162:
                        json = GpBinaryV162;
                        goto Label_0101;

                    case ProtocolType.Json:
                        json = Json;
                        goto Label_0101;
                }
                log.WarnFormat("Subprotocol {0} not supported", new object[] { str2 });
                return false;
            }
        Label_0101:
            initRequest = new InitRequest(applicationName, clientVersion, json);
            return true;
        }

        /// <summary>
        /// Tries to register a custom type for serialisation.
        /// </summary>
        /// <param name="customType">Type of the custom type.</param>
        /// <param name="typeCode">The type code.</param>
        /// <param name="serializeFunction">The serialize function.</param>
        /// <param name="deserializeFunction">The deserialize function.</param>
        /// <returns>
        /// True if the custom type was successfully registered; otherwise false.
        /// False will be returned if either the type or the type code allready is registered.
        /// </returns>
        public static bool TryRegisterCustomType(Type customType, byte typeCode, Func<object, byte[]> serializeFunction, Func<byte[], object> deserializeFunction)
        {
            return CustomTypeCache.TryRegisterType(customType, typeCode, serializeFunction, deserializeFunction);
        }

        /// <summary>
        /// Gets or sets a value indicating whether unknown customes types are allowed.
        ///     if set to true unknown custom types will be serialized as an <
        ///     see cref="T:Photon.SocketServer.Rpc.ValueTypes.RawCustomValue"/> instance.
        /// </summary>
        /// <value><c>true</c> if unknown custom types are allowed; otherwise, <c>false</c>.</value>
        public static bool AllowRawCustomValues { get; set; }

        /// <summary>
        /// Gets the Amf3 protocol version 1.6 with header version 2.
        /// </summary>
        public static IRpcProtocol Amf3V162
        {
            get
            {
                return Amf3Protocol.HeaderV2Instance;
            }
        }

        /// <summary>
        /// Gets the GpBinary protocol version 1.5 with header version 2
        /// </summary>
        public static IRpcProtocol GpBinaryV152
        {
            get
            {
                return GpBinaryByteProtocol.HeaderV2Instance;
            }
        }

        /// <summary>
        /// Gets the GpBinary protocol version 1.6 with header version 2
        /// </summary>
        public static IRpcProtocol GpBinaryV162
        {
            get
            {
                return GpBinaryByteProtocolV16.HeaderV2Instance;
            }
        }

        /// <summary>
        /// Gets the GpBinary protocol version 1.7
        /// </summary>
        public static IRpcProtocol GpBinaryV17
        {
            get
            {
                return GpBinaryProtocolV17.Instance;
            }
        }

        /// <summary>
        /// Gets the Json protocol implementation.
        /// </summary>
        public static IRpcProtocol Json
        {
            get
            {
                return JsonProtocol.Instance;
            }
        }
    }
}
