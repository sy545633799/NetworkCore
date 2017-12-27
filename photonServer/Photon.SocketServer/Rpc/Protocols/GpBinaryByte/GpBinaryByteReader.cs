using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using ExitGames.IO;
using ExitGames.Logging;
using Photon.SocketServer.Rpc.ValueTypes;

namespace Photon.SocketServer.Rpc.Protocols.GpBinaryByte
{
    /// <summary>
    /// gp reader.
    /// </summary>
    internal class GpBinaryByteReader
    {
        /// <summary>
        /// The log.
        /// </summary>
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        // Methods
        private static Type GetDictArrayType(IBinaryReader reader)
        {
            GpType gpType = (GpType)reader.ReadByte();
            int num = 0;
            while (gpType == GpType.Array)
            {
                num++;
                gpType = (GpType)reader.ReadByte();
            }
            Type type3 = GpBinaryByteTypeConverter.GetClrArrayType(gpType).MakeArrayType();
            for (int i = 0; i < num; i++)
            {
                type3 = type3.MakeArrayType();
            }
            return type3;
        }

        private static ReadDelegate GetReadDelegate(GpType gpType)
        {
            switch (gpType)
            {
                case GpType.StringArray:
                    return new ReadDelegate(GpBinaryByteReader.ReadStringArray);

                case GpType.Byte:
                    return new ReadDelegate(GpBinaryByteReader.ReadByte);

                case GpType.Custom:
                    return new ReadDelegate(GpBinaryByteReader.ReadCustomType);

                case GpType.Double:
                    return new ReadDelegate(GpBinaryByteReader.ReadDouble);

                case GpType.Float:
                    return new ReadDelegate(GpBinaryByteReader.ReadSingle);

                case GpType.Hashtable:
                    return new ReadDelegate(GpBinaryByteReader.ReadHashTable);

                case GpType.Integer:
                    return new ReadDelegate(GpBinaryByteReader.ReadInt32);

                case GpType.Short:
                    return new ReadDelegate(GpBinaryByteReader.ReadInt16);

                case GpType.Long:
                    return new ReadDelegate(GpBinaryByteReader.ReadInt64);

                case GpType.IntegerArray:
                    return new ReadDelegate(GpBinaryByteReader.ReadIntArray);

                case GpType.Boolean:
                    return new ReadDelegate(GpBinaryByteReader.ReadBool);

                case GpType.String:
                    return new ReadDelegate(GpBinaryByteReader.ReadString);

                case GpType.Vector:
                    return new ReadDelegate(GpBinaryByteReader.ReadVector);

                case GpType.ByteArray:
                    return new ReadDelegate(GpBinaryByteReader.ReadByteArray);

                case GpType.Array:
                    return new ReadDelegate(GpBinaryByteReader.ReadArray);

                case GpType.ObjectArray:
                    return new ReadDelegate(GpBinaryByteReader.ReadObjectArray);

                case GpType.Dictionary:
                    return new ReadDelegate(GpBinaryByteReader.ReadDictionary);
            }
            return null;
        }

        /// <summary>
        /// Reads an object from a binary reader.
        /// </summary>
        /// <param name="binaryReader">The binary Reader.</param>
        /// <param name="result">The result.</param>
        /// <returns>The next object read from the binary reader.</returns>
        public static bool Read(IBinaryReader binaryReader, out object result)
        {
            GpType protocolType = (GpType)binaryReader.ReadByte();
            return Read(binaryReader, protocolType, out result);
        }

        /// <summary>
        /// The read.
        /// </summary>
        /// <param name="binaryReader"> The binary reader.</param>
        /// <param name="protocolType">The protocol type.</param>
        /// <param name="result"> The result.</param>
        /// <returns>error 1 or ok</returns>
        private static bool Read(IBinaryReader binaryReader, GpType protocolType, out object result)
        {
            switch (protocolType)
            {
                case GpType.StringArray:
                    return ReadStringArray(binaryReader, out result);

                case GpType.Byte:
                    return ReadByte(binaryReader, out result);

                case GpType.Custom:
                    return ReadCustomType(binaryReader, out result);

                case GpType.Double:
                    return ReadDouble(binaryReader, out result);

                case GpType.EventData:
                    return ReadEventData(binaryReader, out result);

                case GpType.Float:
                    return ReadSingle(binaryReader, out result);

                case GpType.Hashtable:
                    return ReadHashTable(binaryReader, out result);

                case GpType.Integer:
                    return ReadInt32(binaryReader, out result);

                case GpType.Short:
                    return ReadInt16(binaryReader, out result);

                case GpType.Long:
                    return ReadInt64(binaryReader, out result);

                case GpType.IntegerArray:
                    return ReadIntArray(binaryReader, out result);

                case GpType.Boolean:
                    return ReadBool(binaryReader, out result);

                case GpType.OperationResponse:
                    return ReadOperationResponse(binaryReader, out result);

                case GpType.OperationRequest:
                    return ReadOperationRequest(binaryReader, out result);

                case GpType.String:
                    return ReadString(binaryReader, out result);

                case GpType.Vector:
                    return ReadVector(binaryReader, out result);

                case GpType.ByteArray:
                    return ReadByteArray(binaryReader, out result);

                case GpType.Array:
                    return ReadArray(binaryReader, out result);

                case GpType.ObjectArray:
                    return ReadObjectArray(binaryReader, out result);

                case GpType.Dictionary:
                    return ReadDictionary(binaryReader, out result);

                case GpType.Null:
                    result = null;
                    return true;
            }
            if (log.IsDebugEnabled)
            {
                log.DebugFormat("Unknown GpType {0} at position {1} at {2}", new object[] { protocolType, binaryReader.BaseStream.Position, new StackTrace(true) });
            }
            result = null;
            return false;
        }

        /// <summary>
        /// Reads an array from a specified <see cref="T:ExitGames.IO.IBinaryReader"/>
        /// </summary>
        /// <param name="binaryReader">The <see cref="T:ExitGames.IO.IBinaryReader"/> to read from.</param>
        /// <param name="result">When this method returns true, contains the array that was read from the <see cref="T:ExitGames.IO.IBinaryReader"/>.</param>
        /// <returns>True if the value was successfully read; otherwise false.</returns>
        internal static bool ReadArray(IBinaryReader binaryReader, out object result)
        {
            short size = binaryReader.ReadInt16();
            byte num2 = binaryReader.ReadByte();
            GpType gpType = (GpType)num2;
            if (gpType == GpType.Dictionary)
            {
                return ReadDictionaryArray(binaryReader, size, out result);
            }
            int num3 = 1;
            Type clrArrayType = GpBinaryByteTypeConverter.GetClrArrayType(gpType, ref num3);
            if ((size < 0) || ((size * num3) > (binaryReader.BaseStream.Length - binaryReader.BaseStream.Position)))
            {
                if (log.IsDebugEnabled)
                {
                    log.DebugFormat("Invalid length for array of type {2}: length={0}, bytesLeft={1}", new object[] { size, binaryReader.BaseStream.Length - binaryReader.BaseStream.Position, gpType });
                }
                result = null;
                return false;
            }
            switch (gpType)
            {
                case GpType.Custom:
                    return ReadCustomTypeArray(binaryReader, size, out result);

                case GpType.Array:
                    return ReadArrayInArray(binaryReader, size, out result);
            }
            if (clrArrayType == null)
            {
                if (log.IsDebugEnabled)
                {
                    if (gpType == GpType.Unknown)
                    {
                        log.DebugFormat("Array of unknown type {0} is not supported.", new object[] { num2 });
                    }
                    else
                    {
                        log.DebugFormat("Array of type {0} is not supported.", new object[] { gpType });
                    }
                }
                result = null;
                return false;
            }
            Array array = Array.CreateInstance(clrArrayType, size);
            for (short i = 0; i < size; i = (short)(i + 1))
            {
                object obj2;
                if (Read(binaryReader, gpType, out obj2))
                {
                    array.SetValue(obj2, (int)i);
                }
                else
                {
                    result = null;
                    return false;
                }
            }
            result = array;
            return true;
        }

        private static bool ReadArrayInArray(IBinaryReader reader, int size, out object result)
        {
            object obj2;
            if (!ReadArray(reader, out obj2))
            {
                result = null;
                return false;
            }
            Array array = obj2 as Array;
            if (array != null)
            {
                Array array2 = Array.CreateInstance(array.GetType(), size);
                array2.SetValue(array, 0);
                for (short i = 1; i < size; i = (short)(i + 1))
                {
                    if (ReadArray(reader, out obj2))
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
            RawCustomArray array3 = obj2 as RawCustomArray;
            if (array3 != null)
            {
                Array array4 = new RawCustomArray[size];
                array4.SetValue(array3, 0);
                for (int j = 1; j < size; j++)
                {
                    if (!ReadArray(reader, out obj2))
                    {
                        result = null;
                        return false;
                    }
                    array3 = obj2 as RawCustomArray;
                    if (array3 != null)
                    {
                        array4.SetValue(array3, j);
                    }
                }
                result = array4;
                return true;
            }
            result = null;
            return false;
        }

        /// <summary>
        /// Reads a boolean value from a specified <see cref="T:ExitGames.IO.IBinaryReader"/>
        /// </summary>
        /// <param name="binaryReader">The <see cref="T:ExitGames.IO.IBinaryReader"/> to read from.</param>
        /// <param name="result">When this method returns true, contains the value that was read from the <see cref="T:ExitGames.IO.IBinaryReader"/>.</param>
        /// <returns>True if the value was successfully read; otherwise false.</returns>
        public static bool ReadBool(IBinaryReader binaryReader, out object result)
        {
            result = binaryReader.ReadBoolean();
            return true;
        }

        /// <summary>
        /// Reads a byte value from a specified <see cref="T:ExitGames.IO.IBinaryReader"/>
        /// </summary>
        /// <param name="binaryReader">The <see cref="T:ExitGames.IO.IBinaryReader"/> to read from.</param>
        /// <param name="result">When this method returns true, contains the value that was read from the <see cref="T:ExitGames.IO.IBinaryReader"/>.</param>
        /// <returns>True if the value was successfully read; otherwise false.</returns>
        public static bool ReadByte(IBinaryReader binaryReader, out object result)
        {
            result = binaryReader.ReadByte();
            return true;
        }

        /// <summary>
        /// Reads a byte array from a specified <see cref="T:ExitGames.IO.IBinaryReader"/>
        /// </summary>
        /// <param name="reader">The <see cref="T:ExitGames.IO.IBinaryReader"/> to read from.</param>
        /// <param name="byteArray">When this method returns true, contains the byte array that was read from the <see cref="T:ExitGames.IO.IBinaryReader"/>.</param>
        /// <returns>True if the byte array was successfully read; otherwise false.</returns>
        public static bool ReadByteArray(IBinaryReader reader, out object byteArray)
        {
            int length = reader.ReadInt32();
            if ((length < 0) || ((reader.BaseStream.Length - reader.BaseStream.Position) < length))
            {
                if (log.IsDebugEnabled)
                {
                    log.DebugFormat("Invalid length for byte array: length={0}, bytesLeft={1}", new object[] { length, reader.BaseStream.Length - reader.BaseStream.Position });
                }
                byteArray = null;
                return false;
            }
            byteArray = reader.ReadBytes(length);
            return true;
        }

        /// <summary>
        /// Tries to read a custom type.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="result">The result.</param>
        /// <returns>True if the custom type could be readed successfully; otherwise false.</returns>
        public static bool ReadCustomType(IBinaryReader reader, out object result)
        {
            CustomTypeInfo info;
            byte typeCode = reader.ReadByte();
            if (!CustomTypeCache.TryGet(typeCode, out info) && !Protocol.AllowRawCustomValues)
            {
                if (log.IsDebugEnabled)
                {
                    log.DebugFormat("Could not find custom type for type code {0}", new object[] { typeCode });
                }
                result = null;
                return false;
            }
            short length = reader.ReadInt16();
            if ((length < 0) || ((reader.BaseStream.Length - reader.BaseStream.Position) < length))
            {
                if (log.IsDebugEnabled)
                {
                    log.DebugFormat("Invalid length for custom type: length={0}, bytesLeft={1}", new object[] { length, reader.BaseStream.Length - reader.BaseStream.Position });
                }
                result = null;
                return false;
            }
            byte[] arg = reader.ReadBytes(length);
            result = (info != null) ? info.DeserializeFunction(arg) : new RawCustomValue(typeCode, arg);
            return true;
        }

        private static bool ReadCustomTypeArray(IBinaryReader reader, int size, out object result)
        {
            CustomTypeInfo info;
            byte typeCode = reader.ReadByte();
            if (!CustomTypeCache.TryGet(typeCode, out info) && !Protocol.AllowRawCustomValues)
            {
                if (log.IsDebugEnabled)
                {
                    log.DebugFormat("Could not find custom type for type code {0}", new object[] { typeCode });
                }
                result = null;
                return false;
            }
            if ((size < 0) || ((size * 2) > (reader.BaseStream.Length - reader.BaseStream.Position)))
            {
                if (log.IsDebugEnabled)
                {
                    log.DebugFormat("Invalid length for array of type {2}: length={0}, bytesLeft={1}", new object[] { size, reader.BaseStream.Length - reader.BaseStream.Position, info.Type });
                }
                result = null;
                return false;
            }
            if (info != null)
            {
                Array array = Array.CreateInstance(info.Type, size);
                for (short i = 0; i < size; i = (short)(i + 1))
                {
                    int length = reader.ReadInt16();
                    byte[] arg = reader.ReadBytes(length);
                    object obj2 = info.DeserializeFunction(arg);
                    array.SetValue(obj2, (int)i);
                }
                result = array;
            }
            else
            {
                RawCustomArray array2 = new RawCustomArray(typeCode, size);
                for (short j = 0; j < size; j = (short)(j + 1))
                {
                    int num5 = reader.ReadInt16();
                    byte[] buffer2 = reader.ReadBytes(num5);
                    array2[j] = buffer2;
                }
                result = array2;
            }
            return true;
        }

        private static bool ReadDictionary(IBinaryReader reader, out object result)
        {
            ReadDelegate readDelegate;
            Type clrArrayType;
            ReadDelegate delegate3;
            Type dictArrayType;
            result = null;
            GpType gpType = (GpType)reader.ReadByte();
            if (gpType == GpType.Unknown)
            {
                clrArrayType = typeof(object);
                readDelegate = new ReadDelegate(GpBinaryByteReader.Read);
            }
            else
            {
                clrArrayType = GpBinaryByteTypeConverter.GetClrArrayType(gpType);
                readDelegate = GetReadDelegate(gpType);
            }
            GpType type3 = (GpType)reader.ReadByte();
            switch (type3)
            {
                case GpType.Unknown:
                    dictArrayType = typeof(object);
                    delegate3 = new ReadDelegate(GpBinaryByteReader.Read);
                    break;

                case GpType.Dictionary:
                    dictArrayType = ReadDictionaryType(reader);
                    delegate3 = new ReadDelegate(GpBinaryByteReader.ReadDictionary);
                    break;

                case GpType.Array:
                    dictArrayType = GetDictArrayType(reader);
                    delegate3 = new ReadDelegate(GpBinaryByteReader.ReadArray);
                    break;

                case GpType.ObjectArray:
                    dictArrayType = typeof(object[]);
                    delegate3 = new ReadDelegate(GpBinaryByteReader.ReadObjectArray);
                    break;

                case GpType.Custom:
                    CustomTypeInfo info;
                    if (!CustomTypeCache.TryGet(reader.ReadByte(), out info))
                    {
                        return false;
                    }
                    dictArrayType = info.Type;
                    delegate3 = new ReadDelegate(GpBinaryByteReader.ReadCustomType);
                    break;

                default:
                    dictArrayType = GpBinaryByteTypeConverter.GetClrArrayType(type3);
                    delegate3 = GetReadDelegate(type3);
                    if ((dictArrayType == null) || (delegate3 == null))
                    {
                        return false;
                    }
                    break;
            }
            IDictionary dictionary = Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(new Type[] { clrArrayType, dictArrayType })) as IDictionary;
            if (dictionary == null)
            {
                return false;
            }
            short num2 = reader.ReadInt16();
            for (int i = 0; i < num2; i++)
            {
                object obj2;
                object obj3;
                if (!readDelegate(reader, out obj2))
                {
                    return false;
                }
                if (!delegate3(reader, out obj3))
                {
                    return false;
                }
                dictionary.Add(obj2, obj3);
            }
            result = dictionary;
            return true;
        }

        internal static bool ReadDictionaryArray(IBinaryReader reader, short size, out object result)
        {
            ReadDelegate delegate2;
            ReadDelegate delegate3;
            result = null;
            Type elementType = ReadDictionaryType(reader, out delegate2, out delegate3);
            Array array = Array.CreateInstance(elementType, size);
            for (short i = 0; i < size; i = (short)(i + 1))
            {
                IDictionary dictionary = Activator.CreateInstance(elementType) as IDictionary;
                if (dictionary == null)
                {
                    return false;
                }
                short num2 = reader.ReadInt16();
                for (int j = 0; j < num2; j++)
                {
                    object obj2;
                    object obj3;
                    if (!delegate2(reader, out obj2))
                    {
                        return false;
                    }
                    if (!delegate3(reader, out obj3))
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

        private static Type ReadDictionaryType(IBinaryReader reader)
        {
            Type clrArrayType;
            Type dictArrayType;
            GpType gpType = (GpType)reader.ReadByte();
            GpType type2 = (GpType)reader.ReadByte();
            if (gpType == GpType.Unknown)
            {
                clrArrayType = typeof(object);
            }
            else
            {
                clrArrayType = GpBinaryByteTypeConverter.GetClrArrayType(gpType);
            }
            switch (type2)
            {
                case GpType.Unknown:
                    dictArrayType = typeof(object);
                    break;

                case GpType.Dictionary:
                    dictArrayType = ReadDictionaryType(reader);
                    break;

                case GpType.Array:
                    dictArrayType = GetDictArrayType(reader);
                    break;

                default:
                    dictArrayType = GpBinaryByteTypeConverter.GetClrArrayType(type2);
                    break;
            }
            return typeof(Dictionary<,>).MakeGenericType(new Type[] { clrArrayType, dictArrayType });
        }

        private static Type ReadDictionaryType(IBinaryReader reader, out ReadDelegate keyReadDelegate, out ReadDelegate valueReadDelegate)
        {
            Type clrArrayType;
            Type type4;
            GpType gpType = (GpType)reader.ReadByte();
            GpType type2 = (GpType)reader.ReadByte();
            if (gpType == GpType.Unknown)
            {
                clrArrayType = typeof(object);
                keyReadDelegate = new ReadDelegate(GpBinaryByteReader.Read);
            }
            else
            {
                clrArrayType = GpBinaryByteTypeConverter.GetClrArrayType(gpType);
                keyReadDelegate = GetReadDelegate(gpType);
            }
            if (type2 == GpType.Unknown)
            {
                type4 = typeof(object);
                valueReadDelegate = new ReadDelegate(GpBinaryByteReader.Read);
            }
            else
            {
                type4 = GpBinaryByteTypeConverter.GetClrArrayType(type2);
                valueReadDelegate = GetReadDelegate(type2);
            }
            return typeof(Dictionary<,>).MakeGenericType(new Type[] { clrArrayType, type4 });
        }

        /// <summary>
        /// Reads an <see cref="T:System.Double"/> value from a specified <see cref="T:ExitGames.IO.IBinaryReader"/>
        /// </summary>
        /// <param name="binaryReader">The <see cref="T:ExitGames.IO.IBinaryReader"/> to read from.</param>
        /// <param name="result">When this method returns true, contains the value that was read from the <see cref="T:ExitGames.IO.IBinaryReader"/>.</param>
        /// <returns>True if the value was successfully read; otherwise false.</returns>
        public static bool ReadDouble(IBinaryReader binaryReader, out object result)
        {
            result = binaryReader.ReadDouble();
            return true;
        }

        public static bool ReadEventData(IBinaryReader binaryReader, out object result)
        {
            byte num = binaryReader.ReadByte();
            short capacity = binaryReader.ReadInt16();
            if ((capacity < 0) || (capacity > ((binaryReader.BaseStream.Length - binaryReader.BaseStream.Position) / 2L)))
            {
                if (log.IsDebugEnabled)
                {
                    log.DebugFormat("Invalid parameter count: count={0}, bytesLeft={1}", new object[] { capacity, binaryReader.BaseStream.Length - binaryReader.BaseStream.Position });
                }
                result = null;
                return false;
            }
            Dictionary<byte, object> dictionary = new Dictionary<byte, object>(capacity);
            for (short i = 0; i < capacity; i = (short)(i + 1))
            {
                object obj2;
                byte num4 = binaryReader.ReadByte();
                if (Read(binaryReader, out obj2))
                {
                    dictionary[num4] = obj2;
                }
                else
                {
                    result = null;
                    return false;
                }
            }
            EventData data = new EventData
            {
                Code = num,
                Parameters = dictionary
            };
            result = data;
            return true;
        }

        /// <summary>
        /// read guid.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns>a guid</returns>
        public static Guid ReadGuid(IBinaryReader reader)
        {
            return new Guid(reader.ReadBytes(0x10));
        }

        /// <summary>
        /// Reads an <see cref="T:System.Collections.Hashtable"/> value from a specified <see cref="T:ExitGames.IO.IBinaryReader"/>
        /// </summary>
        /// <param name="reader">The <see cref="T:ExitGames.IO.IBinaryReader"/> to read from.</param>
        /// <param name="result">When this method returns true, contains the value that was read from the <see cref="T:ExitGames.IO.IBinaryReader"/>.</param>
        /// <returns>True if the value was successfully read; otherwise false.</returns>
        public static bool ReadHashTable(IBinaryReader reader, out object result)
        {
            int capacity = reader.ReadInt16();
            if ((capacity < 0) || ((reader.BaseStream.Length - reader.BaseStream.Position) < (capacity * 2)))
            {
                if (log.IsDebugEnabled)
                {
                    log.DebugFormat("Invalid length Hashtable: length={0}, bytesLeft={1}", new object[] { capacity, reader.BaseStream.Length - reader.BaseStream.Position });
                }
                result = null;
                return false;
            }
            Hashtable hashtable = new Hashtable(capacity);
            for (int i = 0; i < capacity; i++)
            {
                object obj2;
                object obj3;
                if (!Read(reader, out obj2))
                {
                    result = null;
                    return false;
                }
                if (!Read(reader, out obj3))
                {
                    result = null;
                    return false;
                }
                hashtable[obj2] = obj3;
            }
            result = hashtable;
            return true;
        }

        /// <summary>
        /// Reads an Int16 value from a specified <see cref="T:ExitGames.IO.IBinaryReader"/>
        /// </summary>
        /// <param name="binaryReader">The <see cref="T:ExitGames.IO.IBinaryReader"/> to read from.</param>
        /// <param name="result">When this method returns true, contains the value that was read from the <see cref="T:ExitGames.IO.IBinaryReader"/>.</param>
        /// <returns>True if the value was successfully read; otherwise false.</returns>
        public static bool ReadInt16(IBinaryReader binaryReader, out object result)
        {
            result = binaryReader.ReadInt16();
            return true;
        }

        /// <summary>
        /// Reads an Int32 value from a specified <see cref="T:ExitGames.IO.IBinaryReader"/>
        /// </summary>
        /// <param name="binaryReader">The <see cref="T:ExitGames.IO.IBinaryReader"/> to read from.</param>
        /// <param name="result">When this method returns true, contains the value that was read from the <see cref="T:ExitGames.IO.IBinaryReader"/>.</param>
        /// <returns>True if the value was successfully read; otherwise false.</returns>
        public static bool ReadInt32(IBinaryReader binaryReader, out object result)
        {
            result = binaryReader.ReadInt32();
            return true;
        }

        /// <summary>
        /// Reads an <see cref="T:System.Int64"/> value from a specified <see cref="T:ExitGames.IO.IBinaryReader"/>
        /// </summary>
        /// <param name="binaryReader">The <see cref="T:ExitGames.IO.IBinaryReader"/> to read from.</param>
        /// <param name="result">When this method returns true, contains the value that was read from the <see cref="T:ExitGames.IO.IBinaryReader"/>.</param>
        /// <returns>True if the value was successfully read; otherwise false.</returns>
        public static bool ReadInt64(IBinaryReader binaryReader, out object result)
        {
            result = binaryReader.ReadInt64();
            return true;
        }

        /// <summary>
        ///  reads an int array.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="intArray">the result int array</param>
        /// <returns> true on success, otherwise false</returns>
        public static bool ReadIntArray(IBinaryReader reader, out object intArray)
        {
            int num = reader.ReadInt32();
            if ((num < 0) || ((reader.BaseStream.Length - reader.BaseStream.Position) < (num * 4)))
            {
                if (log.IsDebugEnabled)
                {
                    log.DebugFormat("Invalid length for int array: length={0}, bytesLeft={1}", new object[] { num, reader.BaseStream.Length - reader.BaseStream.Position });
                }
                intArray = null;
                return false;
            }
            int[] numArray = new int[num];
            for (int i = 0; i < num; i++)
            {
                numArray[i] = reader.ReadInt32();
            }
            intArray = numArray;
            return true;
        }

        private static bool ReadObjectArray(IBinaryReader reader, out object result)
        {
            short num = reader.ReadInt16();
            if ((num < 0) || ((reader.BaseStream.Length - reader.BaseStream.Position) < num))
            {
                if (log.IsDebugEnabled)
                {
                    log.DebugFormat("Invalid length for object array: length={0}, bytesLeft={1}", new object[] { num, reader.BaseStream.Length - reader.BaseStream.Position });
                }
                result = null;
                return false;
            }
            object[] objArray = new object[num];
            for (short i = 0; i < num; i = (short)(i + 1))
            {
                object obj2;
                if (Read(reader, out obj2))
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

        public static bool ReadOperationRequest(IBinaryReader binaryReader, out object result)
        {
            OperationRequest request = new OperationRequest
            {
                OperationCode = binaryReader.ReadByte()
            };
            short capacity = binaryReader.ReadInt16();
            if ((capacity < 0) || (capacity > ((binaryReader.BaseStream.Length - binaryReader.BaseStream.Position) / 2L)))
            {
                if (log.IsDebugEnabled)
                {
                    log.DebugFormat("Invalid parameter count: count={0}, bytesLeft={1}", new object[] { capacity, binaryReader.BaseStream.Length - binaryReader.BaseStream.Position });
                }
                result = null;
                return false;
            }
            request.Parameters = new Dictionary<byte, object>(capacity);
            for (short i = 0; i < capacity; i = (short)(i + 1))
            {
                object obj2;
                byte num3 = binaryReader.ReadByte();
                if (Read(binaryReader, out obj2))
                {
                    request.Parameters[num3] = obj2;
                }
                else
                {
                    result = null;
                    return false;
                }
            }
            result = request;
            return true;
        }

        public static bool ReadOperationResponse(IBinaryReader binaryReader, out object result)
        {
            string str;
            OperationResponse response = new OperationResponse
            {
                OperationCode = binaryReader.ReadByte(),
                ReturnCode = binaryReader.ReadInt16()
            };
            switch (((GpType)binaryReader.ReadByte()))
            {
                case GpType.Null:
                    str = null;
                    break;

                case GpType.String:
                    if (!ReadString(binaryReader, out str))
                    {
                        result = null;
                        return false;
                    }
                    break;

                default:
                    result = null;
                    return false;
            }
            response.DebugMessage = str;
            short capacity = binaryReader.ReadInt16();
            if ((capacity < 0) || (capacity > ((binaryReader.BaseStream.Length - binaryReader.BaseStream.Position) / 2L)))
            {
                if (log.IsDebugEnabled)
                {
                    log.DebugFormat("Invalid parameter count: count={0}, bytesLeft={1}", new object[] { capacity, binaryReader.BaseStream.Length - binaryReader.BaseStream.Position });
                }
                result = null;
                return false;
            }
            Dictionary<byte, object> dictionary = new Dictionary<byte, object>(capacity);
            for (short i = 0; i < capacity; i = (short)(i + 1))
            {
                object obj2;
                byte key = binaryReader.ReadByte();
                if (Read(binaryReader, out obj2))
                {
                    if (!dictionary.ContainsKey(key))
                    {
                        dictionary[key] = obj2;
                    }
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

        /// <summary>
        /// Reads an <see cref="T:System.Single"/> value from a specified <see cref="T:ExitGames.IO.IBinaryReader"/>
        /// </summary>
        /// <param name="binaryReader">The <see cref="T:ExitGames.IO.IBinaryReader"/> to read from.</param>
        /// <param name="result">When this method returns true, contains the value that was read from the <see cref="T:ExitGames.IO.IBinaryReader"/>.</param>
        /// <returns>True if the value was successfully read; otherwise false.</returns>
        public static bool ReadSingle(IBinaryReader binaryReader, out object result)
        {
            result = binaryReader.ReadSingle();
            return true;
        }

        /// <summary>
        /// Reads an <see cref="T:System.String"/> from a specified <see cref="T:ExitGames.IO.IBinaryReader"/>
        /// </summary>
        /// <param name="reader">The <see cref="T:ExitGames.IO.IBinaryReader"/> to read from.</param>
        /// <param name="result">When this method returns true, contains the string that was read from the <see cref="T:ExitGames.IO.IBinaryReader"/>.</param>
        /// <returns>True if the value was successfully read; otherwise false.</returns>
        public static bool ReadString(IBinaryReader reader, out object result)
        {
            int length = reader.ReadInt16();
            if ((length < 0) || ((reader.BaseStream.Length - reader.BaseStream.Position) < length))
            {
                if (log.IsDebugEnabled)
                {
                    log.DebugFormat("Invalid length for string: length={0}, bytesLeft={1}", new object[] { length, reader.BaseStream.Length - reader.BaseStream.Position });
                }
                result = null;
                return false;
            }
            byte[] bytes = reader.ReadBytes(length);
            result = Encoding.UTF8.GetString(bytes);
            return true;
        }

        /// <summary>
        /// Reads an <see cref="T:System.String"/> from a specified <see cref="T:ExitGames.IO.IBinaryReader"/>
        /// </summary>
        /// <param name="reader">The <see cref="T:ExitGames.IO.IBinaryReader"/> to read from.</param>
        /// <param name="result">When this method returns true, contains the string that was read from the <see cref="T:ExitGames.IO.IBinaryReader"/>.</param>
        /// <returns>True if the value was successfully read; otherwise false.</returns>
        public static bool ReadString(IBinaryReader reader, out string result)
        {
            int length = reader.ReadInt16();
            if ((length < 0) || ((reader.BaseStream.Length - reader.BaseStream.Position) < length))
            {
                if (log.IsDebugEnabled)
                {
                    log.DebugFormat("Invalid length for string: length={0}, bytesLeft={1}", new object[] { length, reader.BaseStream.Length - reader.BaseStream.Position });
                }
                result = null;
                return false;
            }
            byte[] bytes = reader.ReadBytes(length);
            result = Encoding.UTF8.GetString(bytes);
            return true;
        }

        /// <summary>
        /// Reads an array of <see cref="T:System.String"/> objects from a specified <see cref="T:ExitGames.IO.IBinaryReader"/>
        /// </summary>
        /// <param name="reader">The <see cref="T:ExitGames.IO.IBinaryReader"/> to read from.</param>
        /// <param name="result">When this method returns true, contains the array of strings that was read from the <see cref="T:ExitGames.IO.IBinaryReader"/>.</param>
        /// <returns>True if the value was successfully read; otherwise false.</returns>
        public static bool ReadStringArray(IBinaryReader reader, out object result)
        {
            int num = reader.ReadInt16();
            if ((num < 0) || ((reader.BaseStream.Length - reader.BaseStream.Position) < (num * 2)))
            {
                if (log.IsDebugEnabled)
                {
                    log.DebugFormat("Invalid length for string array: length={0}, bytesLeft={1}", new object[] { num, reader.BaseStream.Length - reader.BaseStream.Position });
                }
                result = null;
                return false;
            }
            string[] strArray = new string[num];
            for (int i = 0; i < num; i++)
            {
                string str;
                if (ReadString(reader, out str))
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

        /// <summary>
        /// Reads an <see cref="T:System.Collections.ArrayList"/> objects from a specified <see cref="T:ExitGames.IO.IBinaryReader"/>
        /// </summary>
        /// <param name="reader">The <see cref="T:ExitGames.IO.IBinaryReader"/> to read from.</param>
        /// <param name="result">When this method returns true, contains the <see cref="T:System.Collections.ArrayList"/> that was read from the <see cref="T:ExitGames.IO.IBinaryReader"/>.</param>
        /// <returns>True if the value was successfully read; otherwise false.</returns>
        public static bool ReadVector(IBinaryReader reader, out object result)
        {
            short capacity = reader.ReadInt16();
            if (capacity == 0)
            {
                result = new ArrayList(0);
                return true;
            }
            GpType gpType = (GpType)((byte)reader.ReadChar());
            int gpTypeSize = GpBinaryByteTypeConverter.GetGpTypeSize(gpType);
            if ((capacity < 0) || ((reader.BaseStream.Length - reader.BaseStream.Position) < (capacity * gpTypeSize)))
            {
                if (log.IsDebugEnabled)
                {
                    log.DebugFormat("Invalid length for vector of type {2}: length={0}, bytesLeft={1}", new object[] { capacity, reader.BaseStream.Length - reader.BaseStream.Position, gpType });
                }
                result = null;
                return false;
            }
            ArrayList list = new ArrayList(capacity);
            for (int i = 0; i < capacity; i++)
            {
                object obj2;
                if (Read(reader, gpType, out obj2))
                {
                    list.Add(obj2);
                }
                else
                {
                    result = null;
                    return false;
                }
            }
            result = list;
            return true;
        }

        // Nested Types
        private delegate bool ReadDelegate(IBinaryReader reader, out object result);
    }
}
