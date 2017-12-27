using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ExitGames.Logging;
using Photon.SocketServer.Rpc.Reflection;
using Photon.SocketServer.Security;

namespace Photon.SocketServer.Rpc.Protocols.Json
{
    internal class JsonProtocol : IRpcProtocol
    {
        // Fields
        private static readonly Encoding encoding = Encoding.UTF8;
        public static readonly JsonProtocol Instance = new JsonProtocol();
        internal const string InternalRequestKey = "irq";
        internal const string InternalResponseKey = "irs";
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        // Methods
        protected JsonProtocol()
        {
        }

        private static byte[] GetMessage(IDictionary h)
        {
            StringBuilder sb = new StringBuilder();
            using (StringWriter writer = new StringWriter(sb))
            {
                if (!JsonSerializer.SerializeObject(h, writer))
                {
                    return null;
                }
                writer.Flush();
            }
            string s = string.Format("~m~{0}~m~~j~{1}", sb.Length + 3, sb);
            return encoding.GetBytes(s);
        }

        public void Serialize(Stream stream, object obj)
        {
            throw new NotImplementedException();
        }

        public byte[] SerializeEventData(EventData eventData)
        {
            Hashtable h = new Hashtable();
            h.Add("evt", eventData.Code);
            if (eventData.Parameters != null)
            {
                List<object> list = new List<object>(eventData.Parameters.Count * 2);
                foreach (KeyValuePair<byte, object> pair in eventData.Parameters)
                {
                    list.Add(pair.Key);
                    list.Add(pair.Value);
                }
                h.Add("vals", list);
            }
            return GetMessage(h);
        }

        public byte[] SerializeEventDataEncrypted(IEventData eventData, ICryptoProvider cryptoProvider)
        {
            throw new NotImplementedException();
        }

        public byte[] SerializeInitRequest(string appName, Version version)
        {
            throw new NotImplementedException();
        }

        public byte[] SerializeInitResponse()
        {
            string str = Guid.NewGuid().ToString();
            string s = string.Format("~m~{0}~m~{1}", str.Length, str);
            return encoding.GetBytes(s);
        }

        public byte[] SerializeInternalOperationRequest(OperationRequest operationRequest)
        {
            Hashtable h = new Hashtable();
            h.Add("irq", operationRequest.OperationCode);
            if (operationRequest.Parameters != null)
            {
                List<object> list = new List<object>(operationRequest.Parameters.Count * 2);
                foreach (KeyValuePair<byte, object> pair in operationRequest.Parameters)
                {
                    list.Add(pair.Key);
                    list.Add(pair.Value);
                }
                h.Add("vals", list);
            }
            return GetMessage(h);
        }

        public byte[] SerializeInternalOperationResponse(OperationResponse operationResponse)
        {
            Hashtable h = new Hashtable();
            h.Add("irs", operationResponse.OperationCode);
            h.Add("err", operationResponse.ReturnCode);
            h.Add("msg", operationResponse.DebugMessage);
            if (operationResponse.Parameters != null)
            {
                List<object> list = new List<object>(operationResponse.Parameters.Count * 2);
                foreach (KeyValuePair<byte, object> pair in operationResponse.Parameters)
                {
                    list.Add(pair.Key);
                    list.Add(pair.Value);
                }
                h.Add("vals", list);
            }
            return GetMessage(h);
        }

        public byte[] SerializeOperationRequest(OperationRequest operationRequest)
        {
            Hashtable h = new Hashtable();
            h.Add("req", operationRequest.OperationCode);
            if (operationRequest.Parameters != null)
            {
                List<object> list = new List<object>(operationRequest.Parameters.Count * 2);
                foreach (KeyValuePair<byte, object> pair in operationRequest.Parameters)
                {
                    list.Add(pair.Key);
                    list.Add(pair.Value);
                }
                h.Add("vals", list);
            }
            return GetMessage(h);
        }

        public byte[] SerializeOperationRequestEncrypted(OperationRequest operationRequest, ICryptoProvider cryptoProvider)
        {
            throw new NotImplementedException();
        }

        public byte[] SerializeOperationResponse(OperationResponse operationResponse)
        {
            Hashtable h = new Hashtable();
            h.Add("res", operationResponse.OperationCode);
            h.Add("err", operationResponse.ReturnCode);
            h.Add("msg", operationResponse.DebugMessage);
            if (operationResponse.Parameters != null)
            {
                List<object> list = new List<object>(operationResponse.Parameters.Count * 2);
                foreach (KeyValuePair<byte, object> pair in operationResponse.Parameters)
                {
                    list.Add(pair.Key);
                    list.Add(pair.Value);
                }
                h.Add("vals", list);
            }
            return GetMessage(h);
        }

        public byte[] SerializeOperationResponseEncrypted(OperationResponse operationResponse, ICryptoProvider cryptoProvider)
        {
            throw new NotImplementedException();
        }

        public bool TryConvertParameter(ObjectMemberInfo<DataMemberAttribute> parameterInfo, ref object value)
        {
            return JsonParameterConverter.TryConvertOperationParameter(parameterInfo, ref value);
        }

        private static bool TryGetValue<T>(IDictionary<string, object> dict, string name, bool required, ref T value)
        {
            object obj2;
            if (!dict.TryGetValue(name, out obj2))
            {
                if (!required)
                {
                    return true;
                }
                if (log.IsDebugEnabled)
                {
                    log.DebugFormat("Received invalid websocket request: {0} parameter is missing.", new object[] { name });
                }
                value = default(T);
                return false;
            }
            if (obj2 is T)
            {
                value = (T)obj2;
                return true;
            }
            if (log.IsDebugEnabled)
            {
                log.DebugFormat("Received invalid websocket request: {0} parameter has wrong type.", new object[] { name });
            }
            return false;
        }

        public bool TryParse(Stream stream, out object obj)
        {
            throw new NotImplementedException();
        }

        public bool TryParseData(byte[] data, string[] requiredKeys, out Dictionary<byte, object> parameters, out Dictionary<string, object> requiredKeysAndValues)
        {
            parameters = null;
            requiredKeysAndValues = new Dictionary<string, object>(requiredKeys.Length);
            try
            {
                long num2;
                Dictionary<string, object> dictionary;
                int num4;
                string str = Encoding.UTF8.GetString(data);
                if (!str.StartsWith("~m~"))
                {
                    if (log.IsDebugEnabled)
                    {
                        log.DebugFormat("Received invalid websocket request: {0}", new object[] { str });
                    }
                    return false;
                }
                int num = str.IndexOf("~m~", 3, StringComparison.InvariantCulture);
                if (num < 0)
                {
                    if (log.IsDebugEnabled)
                    {
                        log.DebugFormat("Received invalid websocket request: {0}", new object[] { str });
                    }
                    return false;
                }
                if (!long.TryParse(str.Substring(3, num - 3), out num2))
                {
                    if (log.IsDebugEnabled)
                    {
                        log.DebugFormat("Received invalid websocket request: {0}", new object[] { str });
                    }
                    return false;
                }
                int num3 = str.IndexOf("~j~", num + 3, StringComparison.InvariantCulture);
                if (num3 != (num + 3))
                {
                    if (log.IsDebugEnabled)
                    {
                        log.DebugFormat("Received invalid websocket request: {0}", new object[] { str });
                    }
                    return false;
                }
                string s = str.Substring(num3 + 3);
                if (!JsonSerializer.TryDeserialize(s, out dictionary, out num4))
                {
                    if (log.IsDebugEnabled)
                    {
                        log.DebugFormat("Invalid request: Unexpected token at position {0} - json={1}", new object[] { num4, s });
                    }
                    return false;
                }
                foreach (string str4 in requiredKeys)
                {
                    object obj2 = null;
                    if (!TryGetValue<object>(dictionary, str4, true, ref obj2))
                    {
                        return false;
                    }
                    requiredKeysAndValues[str4] = obj2;
                }
                object[] objArray = null;
                if (!TryGetValue<object[]>(dictionary, "vals", false, ref objArray))
                {
                    return false;
                }
                if ((objArray.Length % 2) != 0)
                {
                    log.DebugFormat("Invalid request: Unexpected parameter count ", new object[0]);
                    return false;
                }
                parameters = new Dictionary<byte, object>(objArray.Length);
                for (int i = 0; i < objArray.Length; i += 2)
                {
                    if (!(objArray[i] is double))
                    {
                        log.DebugFormat("Invalid request: Parameter code {0} is not convertable to byte.", new object[] { objArray[i] });
                        return false;
                    }
                    byte num6 = (byte)((double)objArray[i]);
                    parameters[num6] = objArray[i + 1];
                }
                return true;
            }
            catch (Exception exception)
            {
                log.Debug(exception);
                return false;
            }
        }

        public bool TryParseEventData(byte[] data, out EventData eventData)
        {
            Dictionary<string, object> dictionary;
            Dictionary<byte, object> dictionary2;
            eventData = null;
            if (!this.TryParseData(data, new string[] { "evt" }, out dictionary2, out dictionary))
            {
                return false;
            }
            object obj2 = dictionary["evt"];
            if (!(obj2 is double))
            {
                if (log.IsDebugEnabled)
                {
                    log.DebugFormat("Received invalid websocket request: \"evt\" is not convertible to byte: {0}", new object[] { obj2 });
                }
                return false;
            }
            EventData data2 = new EventData
            {
                Code = (byte)((double)obj2),
                Parameters = dictionary2
            };
            eventData = data2;
            return true;
        }

        public bool TryParseEventDataEncrypted(byte[] data, ICryptoProvider cryptoProvider, out EventData eventData)
        {
            throw new NotImplementedException();
        }

        public bool TryParseInternalOperationRequest(byte[] data, out OperationRequest operationRequest)
        {
            Dictionary<byte, object> dictionary;
            Dictionary<string, object> dictionary2;
            operationRequest = null;
            if (!this.TryParseData(data, new string[] { "irq" }, out dictionary, out dictionary2))
            {
                return false;
            }
            object obj2 = dictionary2["irq"];
            if (!(obj2 is double))
            {
                if (log.IsDebugEnabled)
                {
                    log.DebugFormat("Received invalid websocket request: \"irq\" is not convertible to byte: {0}", new object[] { obj2 });
                }
                return false;
            }
            OperationRequest request = new OperationRequest
            {
                OperationCode = (byte)((double)obj2),
                Parameters = dictionary
            };
            operationRequest = request;
            return true;
        }

        public bool TryParseInternalOperationResponse(byte[] data, out OperationResponse operationResponse)
        {
            Dictionary<string, object> dictionary;
            Dictionary<byte, object> dictionary2;
            operationResponse = null;
            if (!this.TryParseData(data, new string[] { "irs", "err", "msg" }, out dictionary2, out dictionary))
            {
                return false;
            }
            object obj2 = dictionary["irs"];
            if (!(obj2 is double))
            {
                if (log.IsDebugEnabled)
                {
                    log.DebugFormat("Received invalid websocket request: \"irs\" is not convertible to byte: {0}", new object[] { obj2 });
                }
                return false;
            }
            object obj3 = dictionary["err"];
            if (!(obj3 is double))
            {
                if (log.IsDebugEnabled)
                {
                    log.DebugFormat("Received invalid websocket request: \"err\" is not convertible to byte: {0}", new object[] { obj3 });
                }
                return false;
            }
            object obj4 = dictionary["msg"];
            if (!(obj4 is string))
            {
                if (log.IsDebugEnabled)
                {
                    log.DebugFormat("Received invalid websocket request: \"msg\" is not of type string: {0}", new object[] { obj3 });
                }
                return false;
            }
            OperationResponse response = new OperationResponse
            {
                OperationCode = (byte)((double)obj2),
                ReturnCode = (byte)((double)obj3),
                Parameters = dictionary2,
                DebugMessage = (string)obj4
            };
            operationResponse = response;
            return true;
        }

        public bool TryParseMessageHeader(byte[] data, out RtsMessageHeader header)
        {
            int num2;
            header = new RtsMessageHeader();
            string str = Encoding.UTF8.GetString(data);
            if (!str.StartsWith("~m~"))
            {
                if (log.IsDebugEnabled)
                {
                    log.DebugFormat("Received invalid websocket request - should start with ~m~, but was: {0}", new object[] { str });
                }
                return false;
            }
            string str2 = str.Substring(3, str.Length - 3);
            int index = str2.IndexOf("~m~", StringComparison.Ordinal);
            if (index < 0)
            {
                if (log.IsDebugEnabled)
                {
                    log.DebugFormat("Received invalid websocket request - should start with ~m~[sessionIdLength]~m~, but was: {0}", new object[] { str });
                }
                return false;
            }
            if (!int.TryParse(str2.Substring(0, index), out num2))
            {
                if (log.IsDebugEnabled)
                {
                    log.DebugFormat("Received invalid websocket request - invalid session ID length. Should start with ~m~[sessionIdLength]~m~, but was: {0}", new object[] { str });
                }
                return false;
            }
            string str3 = str2.Substring(index + 3, (str2.Length - index) - 3);
            if (num2 != str3.Length)
            {
                if (log.IsDebugEnabled)
                {
                    log.DebugFormat("Received invalid websocket request - data length should be {0}, but was {1}: {2}", new object[] { num2, str3.Length, str });
                }
                return false;
            }
            if (str3.StartsWith("~j~"))
            {
                if (str3.Contains("\"evt\":"))
                {
                    header.MessageType = RtsMessageType.Event;
                }
                else if (str3.Contains("\"res\":"))
                {
                    header.MessageType = RtsMessageType.OperationResponse;
                }
                else if (str3.Contains("\"req\":"))
                {
                    header.MessageType = RtsMessageType.Operation;
                }
                else if (str3.Contains("\"irq\":"))
                {
                    header.MessageType = RtsMessageType.InternalOperationRequest;
                }
                else if (str3.Contains("\"irs\":"))
                {
                    header.MessageType = RtsMessageType.InternalOperationRequest;
                }
                else
                {
                    if (log.IsDebugEnabled)
                    {
                        log.DebugFormat("Received invalid websocket request - expected value: \"evt\":, \"res\":, \"req\":, \"irq\": or \"irs\": but was {1}: {2}", new object[] { num2, str3.Length, str });
                    }
                    return false;
                }
                return true;
            }
            try
            {
                RtsMessageHeader header2 = new RtsMessageHeader
                {
                    MagicByte = (RtsMagicByte)0,
                    MessageType = RtsMessageType.InitResponse,
                    IsEncrypted = false,
                    SizeInBytes = 0
                };
                header = header2;
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        public bool TryParseOperationRequest(byte[] data, out OperationRequest operationRequest)
        {
            Dictionary<byte, object> dictionary;
            Dictionary<string, object> dictionary2;
            operationRequest = null;
            if (!this.TryParseData(data, new string[] { "req" }, out dictionary, out dictionary2))
            {
                return false;
            }
            object obj2 = dictionary2["req"];
            if (!(obj2 is double))
            {
                if (log.IsDebugEnabled)
                {
                    log.DebugFormat("Received invalid websocket request: \"req\" is not convertible to byte: {0}", new object[] { obj2 });
                }
                return false;
            }
            OperationRequest request = new OperationRequest
            {
                OperationCode = (byte)((double)obj2),
                Parameters = dictionary
            };
            operationRequest = request;
            return true;
        }

        public bool TryParseOperationRequestEncrypted(byte[] data, ICryptoProvider cryptoProvider, out OperationRequest operationRequest)
        {
            throw new NotImplementedException();
        }

        public bool TryParseOperationResponse(byte[] data, out OperationResponse operationResponse)
        {
            Dictionary<string, object> dictionary;
            Dictionary<byte, object> dictionary2;
            operationResponse = null;
            if (!this.TryParseData(data, new string[] { "res", "err", "msg" }, out dictionary2, out dictionary))
            {
                return false;
            }
            object obj2 = dictionary["res"];
            if (!(obj2 is double))
            {
                if (log.IsDebugEnabled)
                {
                    log.DebugFormat("Received invalid websocket request: \"res\" is not convertible to byte: {0}", new object[] { obj2 });
                }
                return false;
            }
            object obj3 = dictionary["err"];
            if (!(obj3 is double))
            {
                if (log.IsDebugEnabled)
                {
                    log.DebugFormat("Received invalid websocket request: \"err\" is not convertible to byte: {0}", new object[] { obj3 });
                }
                return false;
            }
            object obj4 = dictionary["msg"];
            if (!(obj4 is string))
            {
                if (log.IsDebugEnabled)
                {
                    log.DebugFormat("Received invalid websocket request: \"msg\" is not of type string: {0}", new object[] { obj3 });
                }
                return false;
            }
            OperationResponse response = new OperationResponse
            {
                OperationCode = (byte)((double)obj2),
                ReturnCode = (byte)((double)obj3),
                Parameters = dictionary2,
                DebugMessage = (string)obj4
            };
            operationResponse = response;
            return true;
        }

        public bool TryParseOperationResponseEncrypted(byte[] data, ICryptoProvider cryptoProvider, out OperationResponse operationResponse)
        {
            throw new NotImplementedException();
        }

        // Properties
        public ProtocolType ProtocolType
        {
            get
            {
                return ProtocolType.Json;
            }
        }
    }
}
