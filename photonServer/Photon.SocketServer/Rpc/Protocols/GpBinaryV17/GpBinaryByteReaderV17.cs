using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using ExitGames.Logging;
using Photon.SocketServer.Rpc.Protocols.GpBinary;
using Photon.SocketServer.Rpc.ValueTypes;

namespace Photon.SocketServer.Rpc.Protocols.GpBinaryV17
{
    internal class GpBinaryByteReaderV17
    {
        // Fields
        private static readonly byte[] boolMasks = new byte[] { 1, 2, 4, 8, 0x10, 0x20, 0x40, 0x80 };
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();
        private static readonly ReadDelegate[] readDelegates = new ReadDelegate[0x100];

        // Methods
        static GpBinaryByteReaderV17()
        {
            readDelegates[0x6f] = new ReadDelegate(BigEndianReader.TryReadBoolean);
            readDelegates[0x62] = new ReadDelegate(BigEndianReader.TryReadByte);
            readDelegates[0x6b] = new ReadDelegate(BigEndianReader.TryReadInt16);
            readDelegates[0x69] = new ReadDelegate(BigEndianReader.TryReadInt32);
            readDelegates[0x6c] = new ReadDelegate(BigEndianReader.TryReadInt64);
            readDelegates[0x66] = new ReadDelegate(BigEndianReader.TryReadSingle);
            readDelegates[100] = new ReadDelegate(BigEndianReader.TryReadDouble);
            readDelegates[0x73] = new ReadDelegate(GpBinaryByteReaderV17.ReadString);
            readDelegates[0x68] = new ReadDelegate(GpBinaryByteReaderV17.ReadHashTable);
            readDelegates[0x44] = new ReadDelegate(GpBinaryByteReaderV17.ReadDictionary);
            readDelegates[0x63] = new ReadDelegate(GpBinaryByteReaderV17.ReadCustomType);
            readDelegates[0x71] = new ReadDelegate(GpBinaryByteReaderV17.ReadOperationRequest);
            readDelegates[0x70] = new ReadDelegate(GpBinaryByteReaderV17.TryReadOperationResponse);
            readDelegates[0x65] = new ReadDelegate(GpBinaryByteReaderV17.ReadEventData);
            readDelegates[1] = new ReadDelegate(GpBinaryByteReaderV17.TryReadInt1);
            readDelegates[2] = new ReadDelegate(GpBinaryByteReaderV17.TryReadInt2);
            readDelegates[3] = new ReadDelegate(GpBinaryByteReaderV17.TryReadCompresssedInt32);
            readDelegates[4] = new ReadDelegate(GpBinaryByteReaderV17.TryReadCompresssedInt64);
            readDelegates[0xef] = new ReadDelegate(GpBinaryByteReaderV17.TryReadBooleanArray);
            readDelegates[120] = new ReadDelegate(GpBinaryByteReaderV17.ReadByteArray);
            readDelegates[0xeb] = new ReadDelegate(GpBinaryByteReaderV17.TryReadInt16Array);
            readDelegates[110] = new ReadDelegate(GpBinaryByteReaderV17.ReadIntArray);
            readDelegates[0xeb] = new ReadDelegate(GpBinaryByteReaderV17.TryReadInt16Array);
            readDelegates[0xe4] = new ReadDelegate(GpBinaryByteReaderV17.TryReadDoubleArray);
            readDelegates[230] = new ReadDelegate(GpBinaryByteReaderV17.TryReadSingleArray);
            readDelegates[0xf3] = new ReadDelegate(GpBinaryByteReaderV17.TryReadStringArray);
            readDelegates[0x7a] = new ReadDelegate(GpBinaryByteReaderV17.ReadObjectArray);
            readDelegates[0xe8] = new ReadDelegate(GpBinaryByteReaderV17.TryReadHashtableArray);
            readDelegates[0xc4] = new ReadDelegate(GpBinaryByteReaderV17.TryReadDictionaryArray);
            readDelegates[0xe3] = new ReadDelegate(GpBinaryByteReaderV17.ReadCustomTypeArray);
            readDelegates[0x83] = new ReadDelegate(GpBinaryByteReaderV17.TryReadCompressedInt32Array);
            readDelegates[0x84] = new ReadDelegate(GpBinaryByteReaderV17.TryReadCompressedInt64Array);
            readDelegates[0x79] = new ReadDelegate(GpBinaryByteReaderV17.TryReadArrayInArray);
        }

        #region   参考
        /**   
         * https://code.google.com/p/protobuf-csharp-port/source/browse/csharp/ProtocolBuffers/CodedInputStream.cs
         * http://www.cnblogs.com/stephen-liu74/archive/2013/01/08/2845994.html
         * */
        /// <summary>
        /// Decode a 32-bit value with ZigZag encoding.
        /// </summary>
        /// <remarks>
        /// ZigZag encodes signed integers into values that can be efficiently
        /// encoded with varint.  (Otherwise, negative values must be 
        /// sign-extended to 64 bits to be varint encoded, thus always taking
        /// 10 bytes on the wire.)
        /// </remarks>   
        private static int DecodeZigZag32(uint n)
        {
            return (int)(n >> 1) ^ -(int)(n & 1);
        }

        /// <summary>
        /// Decode a 32-bit value with ZigZag encoding.
        /// </summary>
        /// <remarks>
        /// ZigZag encodes signed integers into values that can be efficiently
        /// encoded with varint.  (Otherwise, negative values must be 
        /// sign-extended to 64 bits to be varint encoded, thus always taking
        /// 10 bytes on the wire.)
        /// </remarks>
        private static long DecodeZigZag64(ulong n)
        {
            return (long)(n >> 1) ^ -(long)(n & 1L);
        }
        #endregion 

        public static Type GetClrArrayType(GpTypeV17 gpType)
        {
            switch (gpType)
            {
                case GpTypeV17.CompressedInt:
                case GpTypeV17.Integer:
                    return typeof(int);

                case GpTypeV17.CompressedLong:
                case GpTypeV17.Long:
                    return typeof(long);

                case GpTypeV17.Byte:
                    return typeof(byte);

                case GpTypeV17.Double:
                    return typeof(double);

                case GpTypeV17.EventData:
                    return typeof(EventData);

                case GpTypeV17.Float:
                    return typeof(float);

                case GpTypeV17.Hashtable:
                    return typeof(Hashtable);

                case GpTypeV17.Short:
                    return typeof(short);

                case GpTypeV17.Boolean:
                    return typeof(bool);

                case GpTypeV17.OperationResponse:
                    return typeof(OperationResponse);

                case GpTypeV17.OperationRequest:
                    return typeof(OperationRequest);

                case GpTypeV17.String:
                    return typeof(string);

                case GpTypeV17.Vector:
                    return typeof(ArrayList);

                case GpTypeV17.ByteArray:
                    return typeof(byte[]);

                case GpTypeV17.ShortArray:
                    return typeof(short[]);
            }
            return null;
        }

        private static Type GetDictArrayType(byte[] data, ref int offset)
        {
            GpTypeV17 gpType = (GpTypeV17)data[offset++];
            int num = 0;
            while (gpType == GpTypeV17.Array)
            {
                num++;
                gpType = (GpTypeV17)data[offset++];
            }
            Type type2 = GetClrArrayType(gpType).MakeArrayType();
            for (int i = 0; i < num; i++)
            {
                type2 = type2.MakeArrayType();
            }
            return type2;
        }

        public static bool Read(byte[] data, ref int offset, out object result)
        {
            if (offset >= data.Length)
            {
                if (log.IsDebugEnabled)
                {
                    log.DebugFormat("Invalid offset for data specified: offset={0}, length={1}", new object[] { (int)offset, data.Length });
                }
                result = null;
                return false;
            }
            GpTypeV17 protocolType = (GpTypeV17)data[offset];
            offset++;
            return Read(data, ref offset, protocolType, out result);
        }

        private static bool Read(byte[] data, ref int offset, GpTypeV17 protocolType, out object result)
        {
            ReadDelegate delegate2 = readDelegates[(int)protocolType];
            if (delegate2 != null)
            {
                return delegate2(data, ref offset, out result);
            }
            if (log.IsDebugEnabled)
            {
                log.DebugFormat("Unknown GpType {0} at position {1} at {2}", new object[] { protocolType, (int)offset, new StackTrace(true) });
            }
            result = null;
            return false;
        }

        public static bool ReadByteArray(byte[] data, ref int offset, out object byteArray)
        {
            uint num;
            if (!TryReadCompresssedUInt32(data, ref offset, out num))
            {
                byteArray = null;
                return false;
            }
            if (!BigEndianReader.TryReadByteArray(data, ref offset, (int)num, out byteArray) && log.IsDebugEnabled)
            {
                log.DebugFormat("Invalid length for byte array: length={0}, bytesLeft={1}", new object[] { num, data.Length - offset });
            }
            return true;
        }

        public static bool ReadCustomType(byte[] data, ref int offset, out object result)
        {
            byte num;
            CustomTypeInfo info;
            uint num2;
            byte[] buffer;
            if (!BigEndianReader.TryReadByte(data, ref offset, out num))
            {
                result = null;
                return false;
            }
            if (!CustomTypeCache.TryGet(num, out info) && !Protocol.AllowRawCustomValues)
            {
                if (log.IsDebugEnabled)
                {
                    log.DebugFormat("Could not find custom type for type code {0}", new object[] { num });
                }
                result = null;
                return false;
            }
            if (!TryReadCompresssedUInt32(data, ref offset, out num2))
            {
                result = null;
                return false;
            }
            if (!BigEndianReader.TryReadByteArray(data, ref offset, (int)num2, out buffer))
            {
                if (log.IsDebugEnabled)
                {
                    log.DebugFormat("Invalid length for custom type: length={0}, bytesLeft={1}", new object[] { num2, data.Length - offset });
                }
                result = null;
                return false;
            }
            result = (info != null) ? info.DeserializeFunction(buffer) : new RawCustomValue(num, buffer);
            return true;
        }

        private static bool ReadCustomTypeArray(byte[] data1, ref int offset, out object result)
        {
            uint num;
            CustomTypeInfo info;
            if (!TryReadCompresssedUInt32(data1, ref offset, out num))
            {
                if (log.IsDebugEnabled)
                {
                    log.DebugFormat("Could not read size of custom type array custom type", new object[0]);
                }
                result = null;
                return false;
            }
            if (offset >= data1.Length)
            {
                result = null;
                return false;
            }
            byte typeCode = data1[offset++];
            if (!CustomTypeCache.TryGet(typeCode, out info) && !Protocol.AllowRawCustomValues)
            {
                if (log.IsDebugEnabled)
                {
                    log.DebugFormat("Could not find custom type for type code {0}", new object[] { typeCode });
                }
                result = null;
                return false;
            }
            if (info != null)
            {
                Array array = Array.CreateInstance(info.Type, new long[] { (long)num });
                for (short i = 0; i < num; i = (short)(i + 1))
                {
                    uint num4;
                    byte[] buffer;
                    if (!TryReadCompresssedUInt32(data1, ref offset, out num4))
                    {
                        result = null;
                        return false;
                    }
                    if (!BigEndianReader.TryReadByteArray(data1, ref offset, (int)num4, out buffer))
                    {
                        result = null;
                        return false;
                    }
                    object obj2 = info.DeserializeFunction(buffer);
                    array.SetValue(obj2, (int)i);
                }
                result = array;
            }
            else
            {
                RawCustomArray array2 = new RawCustomArray(typeCode, (int)num);
                for (short j = 0; j < num; j = (short)(j + 1))
                {
                    uint num6;
                    byte[] buffer2;
                    if (!TryReadCompresssedUInt32(data1, ref offset, out num6))
                    {
                        result = null;
                        return false;
                    }
                    if (!BigEndianReader.TryReadByteArray(data1, ref offset, (int)num6, out buffer2))
                    {
                        result = null;
                        return false;
                    }
                    array2[j] = buffer2;
                }
                result = array2;
            }
            return true;
        }

        private static bool ReadDictionary(byte[] data, ref int offset, out object result)
        {
            ReadDelegate delegate2;
            ReadDelegate delegate3;
            Type type = ReadDictionaryType(data, ref offset, out delegate2, out delegate3);
            if (type == null)
            {
                result = null;
                return false;
            }
            IDictionary dictionary = Activator.CreateInstance(type) as IDictionary;
            if (dictionary == null)
            {
                if (log.IsDebugEnabled)
                {
                    log.DebugFormat("Cannot read dictionary of type {0}", new object[] { type });
                }
                result = null;
                return false;
            }
            if (!ReadDictionaryElements(data, ref offset, delegate2, delegate3, dictionary))
            {
                result = null;
                return false;
            }
            result = dictionary;
            return true;
        }

        internal static bool ReadDictionaryArray(byte[] data, ref int offset, short size, out object result)
        {
            ReadDelegate delegate2;
            ReadDelegate delegate3;
            result = null;
            Type elementType = ReadDictionaryType(data, ref offset, out delegate2, out delegate3);
            Array array = Array.CreateInstance(elementType, size);
            for (short i = 0; i < size; i = (short)(i + 1))
            {
                short num2;
                IDictionary dictionary = Activator.CreateInstance(elementType) as IDictionary;
                if (dictionary == null)
                {
                    return false;
                }
                if (!BigEndianReader.TryReadInt16(data, ref offset, out num2))
                {
                    return false;
                }
                for (int j = 0; j < num2; j++)
                {
                    object obj2;
                    object obj3;
                    if (!delegate2(data, ref offset, out obj2))
                    {
                        return false;
                    }
                    if (!delegate3(data, ref offset, out obj3))
                    {
                        return false;
                    }
                    dictionary.Add(obj2, obj3);
                }
                array.SetValue(dictionary, (int)i);
            }
            result = array;
            return true;
        }

        private static bool ReadDictionaryElements(byte[] data, ref int offset, ReadDelegate keyReadDelegate, ReadDelegate valueReadDelegate, IDictionary dictionary)
        {
            uint num;
            if (!TryReadCompresssedUInt32(data, ref offset, out num))
            {
                return false;
            }
            for (int i = 0; i < num; i++)
            {
                object obj2;
                object obj3;
                if (!keyReadDelegate(data, ref offset, out obj2))
                {
                    return false;
                }
                if (!valueReadDelegate(data, ref offset, out obj3))
                {
                    return false;
                }
                dictionary.Add(obj2, obj3);
            }
            return true;
        }

        private static Type ReadDictionaryType(byte[] data, ref int offset)
        {
            Type clrArrayType;
            Type dictArrayType;
            if (offset > (data.Length - 2))
            {
                return null;
            }
            GpTypeV17 gpType = (GpTypeV17)data[offset++];
            GpTypeV17 ev2 = (GpTypeV17)data[offset++];
            if (gpType == GpTypeV17.Unknown)
            {
                clrArrayType = typeof(object);
            }
            else
            {
                clrArrayType = GetClrArrayType(gpType);
            }
            switch (ev2)
            {
                case GpTypeV17.Unknown:
                    dictArrayType = typeof(object);
                    break;

                case GpTypeV17.Dictionary:
                    dictArrayType = ReadDictionaryType(data, ref offset);
                    break;

                case GpTypeV17.Array:
                    dictArrayType = GetDictArrayType(data, ref offset);
                    break;

                default:
                    dictArrayType = GetClrArrayType(ev2);
                    break;
            }
            return typeof(Dictionary<,>).MakeGenericType(new Type[] { clrArrayType, dictArrayType });
        }

        private static Type ReadDictionaryType(byte[] data, ref int offset, out ReadDelegate keyReadDelegate, out ReadDelegate valueReadDelegate)
        {
            Type clrArrayType;
            Type dictArrayType;
            if (offset >= (data.Length - 2))
            {
                keyReadDelegate = null;
                valueReadDelegate = null;
                return null;
            }
            GpTypeV17 gpType = (GpTypeV17)data[offset];
            GpTypeV17 ev2 = (GpTypeV17)data[offset + 1];
            offset += 2;
            if (gpType == GpTypeV17.Unknown)
            {
                clrArrayType = typeof(object);
                keyReadDelegate = new ReadDelegate(GpBinaryByteReaderV17.Read);
            }
            else
            {
                clrArrayType = GetClrArrayType(gpType);
                keyReadDelegate = readDelegates[(int)gpType];
            }
            switch (ev2)
            {
                case GpTypeV17.Unknown:
                    dictArrayType = typeof(object);
                    valueReadDelegate = new ReadDelegate(GpBinaryByteReaderV17.Read);
                    break;

                case GpTypeV17.Dictionary:
                    dictArrayType = ReadDictionaryType(data, ref offset);
                    valueReadDelegate = new ReadDelegate(GpBinaryByteReaderV17.ReadDictionary);
                    break;

                case GpTypeV17.Array:
                    dictArrayType = GetDictArrayType(data, ref offset);
                    valueReadDelegate = new ReadDelegate(GpBinaryByteReaderV17.Read);
                    break;

                case GpTypeV17.ObjectArray:
                    dictArrayType = typeof(object[]);
                    valueReadDelegate = new ReadDelegate(GpBinaryByteReaderV17.ReadObjectArray);
                    break;

                default:
                    dictArrayType = GetClrArrayType(ev2);
                    valueReadDelegate = readDelegates[(int)ev2];
                    break;
            }
            return typeof(Dictionary<,>).MakeGenericType(new Type[] { clrArrayType, dictArrayType });
        }

        public static bool ReadEventData(byte[] data, ref int offset, out object result)
        {
            if (offset > (data.Length - 2))
            {
                result = null;
                return false;
            }
            byte num = data[offset];
            byte capacity = data[offset + 1];
            offset += 2;
            Dictionary<byte, object> dictionary = new Dictionary<byte, object>(capacity);
            for (short i = 0; i < capacity; i = (short)(i + 1))
            {
                byte num4;
                object obj2;
                if (!BigEndianReader.TryReadByte(data, ref offset, out num4))
                {
                    result = null;
                    return false;
                }
                if (Read(data, ref offset, out obj2))
                {
                    dictionary[num4] = obj2;
                }
                else
                {
                    result = null;
                    return false;
                }
            }
            EventData data2 = new EventData
            {
                Code = num,
                Parameters = dictionary
            };
            result = data2;
            return true;
        }

        public static bool ReadHashTable(byte[] data, ref int offset, out object result)
        {
            uint num;
            if (!TryReadCompresssedUInt32(data, ref offset, out num))
            {
                result = null;
                return false;
            }
            if ((data.Length - offset) < (num * 2))
            {
                if (log.IsDebugEnabled)
                {
                    log.DebugFormat("Invalid length Hashtable: length={0}, bytesLeft={1}", new object[] { num, data.Length - offset });
                }
                result = null;
                return false;
            }
            Hashtable hashtable = new Hashtable((int)num);
            for (int i = 0; i < num; i++)
            {
                object obj2;
                object obj3;
                if (!Read(data, ref offset, out obj2))
                {
                    result = null;
                    return false;
                }
                if (!Read(data, ref offset, out obj3))
                {
                    result = null;
                    return false;
                }
                hashtable[obj2] = obj3;
            }
            result = hashtable;
            return true;
        }

        public static bool ReadIntArray(byte[] data, ref int offset, out object intArray)
        {
            int num;
            int[] numArray;
            if (!BigEndianReader.TryReadInt32(data, ref offset, out num))
            {
                intArray = null;
                return false;
            }
            if (!BigEndianReader.TryReadInt32Array(data, ref offset, num, out numArray))
            {
                if (log.IsDebugEnabled)
                {
                    log.DebugFormat("Invalid length for int array: length={0}, bytesLeft={1}", new object[] { num, data.Length - offset });
                }
                intArray = null;
                return false;
            }
            intArray = numArray;
            return true;
        }

        private static bool ReadObjectArray(byte[] data, ref int offset, out object result)
        {
            uint num;
            if (!TryReadCompresssedUInt32(data, ref offset, out num))
            {
                result = null;
                return false;
            }
            if ((data.Length - offset) < num)
            {
                if (log.IsDebugEnabled)
                {
                    log.DebugFormat("Invalid length for object array: length={0}, bytesLeft={1}", new object[] { num, data.Length - offset });
                }
                result = null;
                return false;
            }
            object[] objArray = new object[num];
            for (short i = 0; i < num; i = (short)(i + 1))
            {
                object obj2;
                if (Read(data, ref offset, out obj2))
                {
                    objArray[i] = obj2;
                }
                else
                {
                    result = null;
                    return false;
                }
            }
            result = objArray;
            return true;
        }

        public static bool ReadOperationRequest(byte[] data, ref int offset, out object operationRequest)
        {
            if (offset > (data.Length - 2))
            {
                operationRequest = null;
                return false;
            }
            byte num = data[offset];
            byte capacity = data[offset + 1];
            offset += 2;
            Dictionary<byte, object> dictionary = new Dictionary<byte, object>(capacity);
            for (short i = 0; i < capacity; i = (short)(i + 1))
            {
                byte num4;
                object obj2;
                if (!BigEndianReader.TryReadByte(data, ref offset, out num4))
                {
                    operationRequest = null;
                    return false;
                }
                if (Read(data, ref offset, out obj2))
                {
                    dictionary[num4] = obj2;
                }
                else
                {
                    operationRequest = null;
                    return false;
                }
            }
            OperationRequest request = new OperationRequest
            {
                OperationCode = num,
                Parameters = dictionary
            };
            operationRequest = request;
            return true;
        }

        public static bool ReadString(byte[] data, ref int offset, out object result)
        {
            string str;
            if (!ReadString(data, ref offset, out str))
            {
                result = null;
                return false;
            }
            result = str;
            return true;
        }

        public static bool ReadString(byte[] data, ref int offset, out string result)
        {
            uint num;
            if (!TryReadCompresssedUInt32(data, ref offset, out num))
            {
                result = null;
                return false;
            }
            if (BigEndianReader.TryReadString(data, ref offset, (int)num, out result))
            {
                return true;
            }
            if (log.IsDebugEnabled)
            {
                log.DebugFormat("Invalid length for string: length={0}, bytesLeft={1}", new object[] { num, data.Length - offset });
            }
            result = null;
            return false;
        }

        public static bool ReadStringArray(byte[] data, ref int offset, out object result)
        {
            short num;
            if (!BigEndianReader.TryReadInt16(data, ref offset, out num))
            {
                result = null;
                return false;
            }
            if (!BigEndianReader.TryReadInt16Array(data, ref offset, num, out result))
            {
                if (log.IsDebugEnabled)
                {
                    log.DebugFormat("Invalid length for string array: length={0}, bytesLeft={1}", new object[] { num, data.Length - offset });
                }
                result = null;
                return false;
            }
            string[] strArray = new string[num];
            for (int i = 0; i < num; i++)
            {
                string str;
                if (ReadString(data, ref offset, out str))
                {
                    strArray[i] = str;
                }
                else
                {
                    result = null;
                    return false;
                }
            }
            result = strArray;
            return true;
        }

        private static bool TryReadArrayInArray(byte[] data, ref int offset, out object result)
        {
            uint num;
            object obj2;
            if (!TryReadCompresssedUInt32(data, ref offset, out num))
            {
                result = null;
                return false;
            }
            if (!Read(data, ref offset, out obj2))
            {
                result = null;
                return false;
            }
            Array array = obj2 as Array;
            if (array != null)
            {
                Array array2 = Array.CreateInstance(array.GetType(), new long[] { (long)num });
                array2.SetValue(array, 0);
                for (short i = 1; i < num; i = (short)(i + 1))
                {
                    if (Read(data, ref offset, out obj2))
                    {
                        array = (Array)obj2;
                        array2.SetValue(array, (int)i);
                    }
                    else
                    {
                        result = null;
                        return false;
                    }
                }
                result = array2;
                return true;
            }
            result = null;
            return false;
        }

        private static bool TryReadBooleanArray(byte[] data, ref int offset, out object value)
        {
            uint num;
            if (!TryReadCompresssedUInt32(data, ref offset, out num))
            {
                value = null;
                return false;
            }
            bool[] flagArray = new bool[num];
            int num2 = (int)(num / 8);
            int num3 = 0;
            while (num2 > 0)
            {
                if (offset >= data.Length)
                {
                    value = null;
                    return false;
                }
                byte num4 = data[offset];
                flagArray[num3++] = (num4 & 1) == 1;
                flagArray[num3++] = (num4 & 2) == 2;
                flagArray[num3++] = (num4 & 4) == 4;
                flagArray[num3++] = (num4 & 8) == 8;
                flagArray[num3++] = (num4 & 0x10) == 0x10;
                flagArray[num3++] = (num4 & 0x20) == 0x20;
                flagArray[num3++] = (num4 & 0x40) == 0x40;
                flagArray[num3++] = (num4 & 0x80) == 0x80;
                num2--;
                offset++;
            }
            if (num3 < num)
            {
                if (offset >= data.Length)
                {
                    value = null;
                    return false;
                }
                byte num5 = data[offset];
                offset++;
                for (int i = 0; num3 < num; i++)
                {
                    flagArray[num3++] = (num5 & boolMasks[i]) == boolMasks[i];
                }
            }
            value = flagArray;
            return true;
        }

        private static bool TryReadCompressedInt32Array(byte[] data, ref int offset, out object value)
        {
            uint num;
            if (!TryReadCompresssedUInt32(data, ref offset, out num))
            {
                value = null;
                return false;
            }
            int[] numArray = new int[num];
            for (int i = 0; i < num; i++)
            {
                int num3;
                if (!TryReadCompresssedInt32(data, ref offset, out num3))
                {
                    value = null;
                    return false;
                }
                numArray[i] = num3;
            }
            value = numArray;
            return true;
        }

        private static bool TryReadCompressedInt64Array(byte[] data, ref int offset, out object value)
        {
            uint num;
            if (!TryReadCompresssedUInt32(data, ref offset, out num))
            {
                value = null;
                return false;
            }
            long[] numArray = new long[num];
            for (int i = 0; i < num; i++)
            {
                long num3;
                if (!TryReadCompresssedInt64(data, ref offset, out num3))
                {
                    value = null;
                    return false;
                }
                numArray[i] = num3;
            }
            value = numArray;
            return true;
        }

        private static bool TryReadCompresssedInt32(byte[] data, ref int offset, out int value)
        {
            uint num;
            if (!TryReadCompresssedUInt32(data, ref offset, out num))
            {
                value = 0;
                return false;
            }
            value = DecodeZigZag32(num);
            return true;
        }

        private static bool TryReadCompresssedInt32(byte[] data, ref int offset, out object value)
        {
            uint num;
            if (!TryReadCompresssedUInt32(data, ref offset, out num))
            {
                value = null;
                return false;
            }
            value = DecodeZigZag32(num);
            return true;
        }

        private static bool TryReadCompresssedInt64(byte[] data, ref int offset, out long value)
        {
            ulong num;
            if (!TryReadCompresssedUInt64(data, ref offset, out num))
            {
                value = 0L;
                return false;
            }
            value = DecodeZigZag64(num);
            return true;
        }

        private static bool TryReadCompresssedInt64(byte[] data, ref int offset, out object value)
        {
            ulong num;
            if (!TryReadCompresssedUInt64(data, ref offset, out num))
            {
                value = null;
                return false;
            }
            value = DecodeZigZag64(num);
            return true;
        }

        private static bool TryReadCompresssedUInt32(byte[] data, ref int offset, out uint value)
        {
            value = 0;
            int num = 0;
            while (num != 0x23)
            {
                if (offset >= data.Length)
                {
                    return false;
                }
                byte num2 = data[offset];
                offset++;
                value |= (uint) ((num2 & 0x7f) << num);
                num += 7;
                if ((num2 & 0x80) == 0)
                {
                    break;
                }
            }
            return true;
        }

        private static bool TryReadCompresssedUInt64(byte[] data, ref int offset, out ulong value)
        {
            value = 0L;
            int num = 0;
            while (num != 70)
            {
                if (offset >= data.Length)
                {
                    return false;
                }
                byte num2 = data[offset];
                offset++;
                value |= (num2 & (ulong)0x7f) << num;
                num += 7;
                if ((num2 & 0x80) == 0)
                {
                    break;
                }
            }
            return true;
        }

        private static bool TryReadDictionaryArray(byte[] data, ref int offset, out object result)
        {
            ReadDelegate delegate2;
            ReadDelegate delegate3;
            uint num;
            Type elementType = ReadDictionaryType(data, ref offset, out delegate2, out delegate3);
            if (!TryReadCompresssedUInt32(data, ref offset, out num))
            {
                result = null;
                return false;
            }
            IDictionary[] dictionaryArray = (IDictionary[])Array.CreateInstance(elementType, new long[] { (long)num });
            result = dictionaryArray;
            if (num != 0)
            {
                for (int i = 0; i < num; i++)
                {
                    dictionaryArray[i] = (IDictionary)Activator.CreateInstance(elementType);
                    ReadDictionaryElements(data, ref offset, delegate2, delegate3, dictionaryArray[i]);
                }
            }
            return true;
        }

        private static bool TryReadDoubleArray(byte[] data, ref int offset, out object value)
        {
            uint num;
            if (!TryReadCompresssedUInt32(data, ref offset, out num))
            {
                value = null;
                return false;
            }
            return BigEndianReader.TryReadDoubleArray(data, ref offset, (int)num, out value);
        }

        private static bool TryReadHashtableArray(byte[] data, ref int offset, out object result)
        {
            uint num;
            if (!TryReadCompresssedUInt32(data, ref offset, out num))
            {
                result = null;
                return false;
            }
            Hashtable[] hashtableArray = new Hashtable[num];
            for (int i = 0; i < num; i++)
            {
                object obj2;
                if (!ReadHashTable(data, ref offset, out obj2))
                {
                    result = null;
                    return false;
                }
                hashtableArray[i] = (Hashtable)obj2;
            }
            result = hashtableArray;
            return true;
        }

        private static bool TryReadInt1(byte[] data, ref int offset, out object value)
        {
            if (offset >= data.Length)
            {
                value = 0;
                return false;
            }
            value = data[offset];
            offset++;
            return true;
        }

        private static bool TryReadInt16Array(byte[] data, ref int offset, out object value)
        {
            uint num;
            if (!TryReadCompresssedUInt32(data, ref offset, out num))
            {
                value = null;
                return false;
            }
            return BigEndianReader.TryReadInt16Array(data, ref offset, (int)num, out value);
        }

        private static bool TryReadInt2(byte[] data, ref int offset, out object value)
        {
            if (offset > (data.Length - 2))
            {
                value = 0;
                return false;
            }
            value = (data[offset] << 8) | data[offset + 1];
            offset += 2;
            return true;
        }

        public static bool TryReadOperationResponse(byte[] data, ref int pos, out object result)
        {
            object obj2;
            byte num;
            if (pos > (data.Length - 4))
            {
                result = null;
                return false;
            }
            OperationResponse response = new OperationResponse
            {
                OperationCode = data[pos],
                ReturnCode = (short)((data[pos + 1] << 8) | data[pos + 2])
            };
            GpTypeV17 ev = (GpTypeV17)data[pos + 3];
            pos += 4;
            switch (ev)
            {
                case GpTypeV17.Null:
                    obj2 = null;
                    break;

                case GpTypeV17.String:
                    if (!ReadString(data, ref pos, out obj2))
                    {
                        result = null;
                        return false;
                    }
                    break;

                default:
                    result = null;
                    return false;
            }
            response.DebugMessage = (string)obj2;
            if (!BigEndianReader.TryReadByte(data, ref pos, out num))
            {
                result = null;
                return false;
            }
            Dictionary<byte, object> dictionary = new Dictionary<byte, object>(num);
            for (short i = 0; i < num; i = (short)(i + 1))
            {
                byte num3;
                object obj3;
                if (!BigEndianReader.TryReadByte(data, ref pos, out num3))
                {
                    result = null;
                    return false;
                }
                if (Read(data, ref pos, out obj3))
                {
                    dictionary[num3] = obj3;
                }
                else
                {
                    result = null;
                    return false;
                }
            }
            response.Parameters = dictionary;
            result = response;
            return true;
        }

        private static bool TryReadSingleArray(byte[] data, ref int offset, out object value)
        {
            uint num;
            if (!TryReadCompresssedUInt32(data, ref offset, out num))
            {
                value = null;
                return false;
            }
            return BigEndianReader.TryReadSingleArray(data, ref offset, (int)num, out value);
        }

        private static bool TryReadStringArray(byte[] data, ref int offset, out object value)
        {
            uint num;
            if (!TryReadCompresssedUInt32(data, ref offset, out num))
            {
                value = null;
                return false;
            }
            string[] strArray = new string[num];
            for (int i = 0; i < num; i++)
            {
                string str;
                if (!ReadString(data, ref offset, out str))
                {
                    value = null;
                    return false;
                }
                strArray[i] = str;
            }
            value = strArray;
            return true;
        }

        // Nested Types
        private delegate bool ReadDelegate(byte[] data, ref int offset, out object result);
    }
}
