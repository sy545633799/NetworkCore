namespace ExitGames.Client.Photon
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Text;

    /// <summary>
    /// Provides tools for the Exit Games Protocol
    /// </summary>
    public class Protocol
    {
        internal static readonly Dictionary<byte, CustomType> CodeDict = new Dictionary<byte, CustomType>();

        internal static readonly Dictionary<Type, CustomType> TypeDict = new Dictionary<Type, CustomType>();

        /// <summary>
        /// Serialize creates a byte-array from the given object and returns it. 
        /// </summary>
        /// <param name="obj">The object to serialize</param>
        /// <returns>The serialized byte-array</returns>
        public static byte[] Serialize(object obj)
        {
            MemoryStream ms = new MemoryStream();
            Serialize(ms, obj, true);
            return ms.ToArray();
        }

        /// <summary>
        /// Deserialize returns an object reassembled from the given byte-array. 
        /// </summary>
        /// <param name="serializedData">The byte-array to be Deserialized</param>
        /// <returns>The Deserialized object</returns>
        public static object Deserialize(byte[] serializedData)
        {
            MemoryStream memoryStream = new MemoryStream(serializedData);
            return Deserialize(memoryStream, (byte)memoryStream.ReadByte());
        }

        /// <summary>
        /// Calls the correct serialization method for the passed object.
        /// </summary>
        /// <param name="dout"></param>
        /// <param name="serObject"></param>
        /// <param name="setType"></param>
        private static void Serialize(MemoryStream dout, object serObject, bool setType)
        {
            if (serObject == null)
            {
                if (setType)
                {
                    dout.WriteByte((byte)GpType.Null);
                }
            }
            else
            {
                Type type = serObject.GetType();
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.Boolean:
                        SerializeBoolean(dout, (bool)serObject, setType);
                        return;

                    case TypeCode.Byte:
                        SerializeByte(dout, (byte)serObject, setType);
                        return;

                    case TypeCode.Int16:
                        SerializeShort(dout, (short)serObject, setType);
                        return;

                    case TypeCode.Int32:
                        SerializeInteger(dout, (int)serObject, setType);
                        return;

                    case TypeCode.Int64:
                        SerializeLong(dout, (long)serObject, setType);
                        return;

                    case TypeCode.Single:
                        SerializeFloat(dout, (float)serObject, setType);
                        return;

                    case TypeCode.Double:
                        SerializeDouble(dout, (double)serObject, setType);
                        return;

                    case TypeCode.String:
                        SerializeString(dout, (string)serObject, setType);
                        return;
                }
                if (serObject is Hashtable)
                {
                    SerializeHashTable(dout, (Hashtable)serObject, setType);
                }
                else if (type.IsArray)
                {
                    if (serObject is byte[])
                    {
                        SerializeByteArray(dout, (byte[])serObject, setType);
                    }
                    else if (serObject is int[])
                    {
                        SerializeIntArrayOptimized(dout, (int[])serObject, setType);
                    }
                    else if (type.GetElementType() == typeof(object))
                    {
                        SerializeObjectArray(dout, serObject as object[], setType);
                    }
                    else
                    {
                        SerializeArray(dout, (Array)serObject, setType);
                    }
                }
                else if (serObject is IDictionary)
                {
                    SerializeDictionary(dout, (IDictionary)serObject, setType);
                }
                else if (serObject is EventData)
                {
                    SerializeEventData(dout, (EventData)serObject, setType);
                }
                else if (serObject is OperationResponse)
                {
                    SerializeOperationResponse(dout, (OperationResponse)serObject, setType);
                }
                else if (serObject is OperationRequest)
                {
                    SerializeOperationRequest(dout, (OperationRequest)serObject, setType);
                }
                else if (!SerializeCustom(dout, serObject))
                {
                    throw new Exception("cannot serialize(): " + serObject.GetType());
                }
            }
        }

        ///<summary>
        /// Serializes a short typed value into a byte-array (target) starting at the also given targetOffset. 
        /// The altered offset is known to the caller, because it is given via a referenced parameter. 
        ///</summary>
        ///<param name="value">The short value to be serialized</param>
        ///<param name="target">The byte-array to serialize the short to</param>
        ///<param name="targetOffset">The offset in the byte-array</param>
        public static void Serialize(short value, byte[] target, ref int targetOffset)
        {
            target[targetOffset++] = (byte)(value >> 8);
            target[targetOffset++] = (byte)value;
        }

        ///<summary>
        /// Serializes an int typed value into a byte-array (target) starting at the also given targetOffset. 
        /// The altered offset is known to the caller, because it is given via a referenced parameter. 
        ///</summary>
        ///<param name="value">The int value to be serialized</param>
        ///<param name="target">The byte-array to serialize the short to</param>
        ///<param name="targetOffset">The offset in the byte-array</param>
        public static void Serialize(int value, byte[] target, ref int targetOffset)
        {
            target[targetOffset++] = (byte)(value >> 0x18);
            target[targetOffset++] = (byte)(value >> 0x10);
            target[targetOffset++] = (byte)(value >> 8);
            target[targetOffset++] = (byte)value;
        }

        ///<summary>
        /// Serializes an float typed value into a byte-array (target) starting at the also given targetOffset. 
        /// The altered offset is known to the caller, because it is given via a referenced parameter. 
        ///</summary>
        ///<param name="value">The float value to be serialized</param>
        ///<param name="target">The byte-array to serialize the short to</param>
        ///<param name="targetOffset">The offset in the byte-array</param>
        public static void Serialize(float value, byte[] target, ref int targetOffset)
        {
            byte[] data = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
            {
                target[targetOffset++] = data[3];
                target[targetOffset++] = data[2];
                target[targetOffset++] = data[1];
                target[targetOffset++] = data[0];
            }
            else
            {
                target[targetOffset++] = data[0];
                target[targetOffset++] = data[1];
                target[targetOffset++] = data[2];
                target[targetOffset++] = data[3];
            }
        }

        ///<summary>
        /// Deserialize fills the given short typed value with the given byte-array (source) starting at the also given offset. 
        /// The result is placed in a variable (value). There is no need to return a value because the parameter value is given by reference. 
        /// The altered offset is this way also known to the caller. 
        ///</summary>
        ///<param name="value">The short value to deserialized into</param>
        ///<param name="target">The byte-array to deserialize from</param>
        ///<param name="targetOffset">The offset in the byte-array</param>
        public static void Deserialize(out short value, byte[] source, ref int offset)
        {
            value = (short)((source[offset++] << 8) | source[offset++]);
        }

        /// <summary>
        /// DeserializeInteger returns an Integer typed value from the given Memorystream. 
        /// </summary>
        /// <param name="din"></param>
        /// <returns></returns>
        private static int DeserializeInteger(MemoryStream din)
        {
            byte[] data = new byte[4];
            din.Read(data, 0, 4);
            return ((((data[0] << 0x18) | (data[1] << 0x10)) | (data[2] << 8)) | data[3]);
        }

        ///<summary>
        /// Deserialize fills the given int typed value with the given byte-array (source) starting at the also given offset. 
        /// The result is placed in a variable (value). There is no need to return a value because the parameter value is given by reference. 
        /// The altered offset is this way also known to the caller. 
        ///</summary>
        ///<param name="value">The int value to deserialize into</param>
        ///<param name="target">The byte-array to deserialize from</param>
        ///<param name="targetOffset">The offset in the byte-array</param>
        public static void Deserialize(out int value, byte[] source, ref int offset)
        {
            value = (((source[offset++] << 0x18) | (source[offset++] << 0x10)) | (source[offset++] << 8)) | source[offset++];
        }

        ///<summary>
        /// Deserialize fills the given float typed value with the given byte-array (source) starting at the also given offset. 
        /// The result is placed in a variable (value). There is no need to return a value because the parameter value is given by reference. 
        /// The altered offset is this way also known to the caller. 
        ///</summary>
        ///<param name="value">The float value to deserialize</param>
        ///<param name="target">The byte-array to deserialize from</param>
        ///<param name="targetOffset">The offset in the byte-array</param>
        public static void Deserialize(out float value, byte[] source, ref int offset)
        {
            if (BitConverter.IsLittleEndian)
            {
                byte[] data = new byte[4];
                data[3] = source[offset++];
                data[2] = source[offset++];
                data[1] = source[offset++];
                data[0] = source[offset++];
                value = BitConverter.ToSingle(data, 0);
            }
            else
            {
                value = BitConverter.ToSingle(source, offset);
                offset += 4;
            }
        }

        private static Array CreateArrayByType(byte arrayType, short length)
        {
            return Array.CreateInstance(GetTypeOfCode(arrayType), length);
        }

        private static object Deserialize(MemoryStream din, byte type)
        {
            switch (type)
            {
                case (byte)GpType.StringArray:
                    return DeserializeStringArray(din);

                case (byte)GpType.Byte:
                    return DeserializeByte(din);

                case (byte)GpType.Custom:
                    {
                        byte typeCode = (byte)din.ReadByte();
                        return DeserializeCustom(din, typeCode);
                    }
                case (byte)GpType.Double:
                    return DeserializeDouble(din);

                case (byte)GpType.EventData:
                    return DeserializeEventData(din);

                case (byte)GpType.Float:
                    return DeserializeFloat(din);

                case (byte)GpType.Hashtable:
                    return DeserializeHashTable(din);

                case (byte)GpType.Integer:
                    return DeserializeInteger(din);

                case (byte)GpType.Short:
                    return DeserializeShort(din);

                case (byte)GpType.Long:
                    return DeserializeLong(din);

                case (byte)GpType.IntegerArray:
                    return DeserializeIntArray(din);

                case (byte)GpType.Boolean:
                    return DeserializeBoolean(din);

                case (byte)GpType.OperationResponse:
                    return DeserializeOperationResponse(din);

                case (byte)GpType.OperationRequest:
                    return DeserializeOperationRequest(din);

                case (byte)GpType.String:
                    return DeserializeString(din);

                case (byte)GpType.Vector:
                    return DeserializeVector(din);

                case (byte)GpType.ByteArray:
                    return DeserializeByteArray(din);

                case (byte)GpType.Array:
                    return DeserializeArray(din);

                case (byte)GpType.ObjectArray:
                    return DeserializeObjectArray(din);

                case (byte)GpType.Dictionary:
                    return DeserializeDictionary(din);

                case 0:
                case (byte)GpType.Null:
                    return null;
            }
            Debug.WriteLine("missing type: " + type);
            throw new Exception("deserialize(): " + type);
        }

        private static Array DeserializeArray(MemoryStream din)
        {
            Array resultArray;
            Array innerArray;
            short i;
            short arrayLength = DeserializeShort(din);
            byte valuesType = (byte)din.ReadByte();
            switch (valuesType)
            {
                case (byte)GpType.Array:
                    innerArray = DeserializeArray(din);
                    resultArray = Array.CreateInstance(innerArray.GetType(), arrayLength);
                    resultArray.SetValue(innerArray, 0);
                    for (i = 1; i < arrayLength; i = (short)(i + 1))
                    {
                        innerArray = DeserializeArray(din);
                        resultArray.SetValue(innerArray, (int)i);
                    }
                    return resultArray;

                case (byte)GpType.ByteArray:
                    resultArray = Array.CreateInstance(typeof(byte[]), arrayLength);
                    for (i = 0; i < arrayLength; i = (short)(i + 1))
                    {
                        innerArray = DeserializeByteArray(din);
                        resultArray.SetValue(innerArray, (int)i);
                    }
                    return resultArray;

                case (byte)GpType.Custom:
                    {
                        CustomType customType;
                        byte customTypeCode = (byte)din.ReadByte();
                        if (!CodeDict.TryGetValue(customTypeCode, out customType))
                        {
                            throw new Exception("Cannot find deserializer for custom type: " + customTypeCode);
                        }
                        resultArray = Array.CreateInstance(customType.Type, arrayLength);
                        for (i = 0; i < arrayLength; i++)
                        {
                            short objLength = DeserializeShort(din);
                            byte[] bytes = new byte[objLength];
                            din.Read(bytes, 0, objLength);
                            resultArray.SetValue(customType.DeserializeFunction(bytes), i);
                        }
                        return resultArray;
                    }
                case (byte)GpType.Null:
                    {
                        Array result = null;
                        DeserializeDictionaryArray(din, arrayLength, out result);
                        return result;
                    }
            }
            resultArray = CreateArrayByType(valuesType, arrayLength);
            for (i = 0; i < arrayLength; i = (short)(i + 1))
            {
                resultArray.SetValue(Deserialize(din, valuesType), (int)i);
            }
            return resultArray;
        }

        private static bool DeserializeBoolean(MemoryStream din)
        {
            return (din.ReadByte() != 0);
        }

        private static byte DeserializeByte(MemoryStream din)
        {
            return (byte)din.ReadByte();
        }

        private static byte[] DeserializeByteArray(MemoryStream din)
        {
            int size = DeserializeInteger(din);
            byte[] retVal = new byte[size];
            din.Read(retVal, 0, size);
            return retVal;
        }

        private static object DeserializeCustom(MemoryStream din, byte customTypeCode)
        {
            CustomType customType;
            short length = DeserializeShort(din);
            byte[] bytes = new byte[length];
            din.Read(bytes, 0, length);
            if (CodeDict.TryGetValue(customTypeCode, out customType))
            {
                return customType.DeserializeFunction(bytes);
            }
            return null;
        }

        private static IDictionary DeserializeDictionary(MemoryStream din)
        {
            byte keyType = (byte)din.ReadByte();
            byte valType = (byte)din.ReadByte();
            int size = DeserializeShort(din);
            bool readKeyType = (keyType == 0) || (keyType == 0x2a);
            bool readValType = (valType == 0) || (valType == 0x2a);
            Type k = GetTypeOfCode(keyType);
            Type v = GetTypeOfCode(valType);
            IDictionary value = Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(new Type[] { k, v })) as IDictionary;
            for (int i = 0; i < size; i++)
            {
                object serKey = Deserialize(din, readKeyType ? (byte)din.ReadByte() : keyType);
                object serValue = Deserialize(din, readValType ? (byte)din.ReadByte() : valType);
                value.Add(serKey, serValue);
            }
            return value;
        }

        private static bool DeserializeDictionaryArray(MemoryStream din, short size, out Array arrayResult)
        {
            byte keyTypeCode;
            byte valTypeCode;
            Type dictType = DeserializeDictionaryType(din, out keyTypeCode, out valTypeCode);
            arrayResult = Array.CreateInstance(dictType, size);
            for (short i = 0; i < size; i = (short)(i + 1))
            {
                IDictionary dict = Activator.CreateInstance(dictType) as IDictionary;
                if (dict == null)
                {
                    return false;
                }
                short dictSize = DeserializeShort(din);
                for (int j = 0; j < dictSize; j++)
                {
                    object key;
                    byte type;
                    object value;
                    if (keyTypeCode != 0)
                    {
                        key = Deserialize(din, keyTypeCode);
                    }
                    else
                    {
                        type = (byte)din.ReadByte();
                        key = Deserialize(din, type);
                    }
                    if (valTypeCode != 0)
                    {
                        value = Deserialize(din, valTypeCode);
                    }
                    else
                    {
                        type = (byte)din.ReadByte();
                        value = Deserialize(din, type);
                    }
                    dict.Add(key, value);
                }
                arrayResult.SetValue(dict, (int)i);
            }
            return true;
        }

        private static Type DeserializeDictionaryType(MemoryStream reader, out byte keyTypeCode, out byte valTypeCode)
        {
            Type keyClrType;
            Type valueClrType;
            keyTypeCode = (byte)reader.ReadByte();
            valTypeCode = (byte)reader.ReadByte();
            GpType keyType = (GpType)keyTypeCode;
            GpType valueType = (GpType)valTypeCode;
            if (keyType == GpType.Unknown)
            {
                keyClrType = typeof(object);
            }
            else
            {
                keyClrType = GetTypeOfCode(keyTypeCode);
            }
            if (valueType == GpType.Unknown)
            {
                valueClrType = typeof(object);
            }
            else
            {
                valueClrType = GetTypeOfCode(valTypeCode);
            }
            return typeof(Dictionary<,>).MakeGenericType(new Type[] { keyClrType, valueClrType });
        }

        private static double DeserializeDouble(MemoryStream din)
        {
            byte[] data = new byte[8];
            din.Read(data, 0, 8);
            if (BitConverter.IsLittleEndian)
            {
                byte temp0 = data[0];
                byte temp1 = data[1];
                byte temp2 = data[2];
                byte temp3 = data[3];
                data[0] = data[7];
                data[1] = data[6];
                data[2] = data[5];
                data[3] = data[4];
                data[4] = temp3;
                data[5] = temp2;
                data[6] = temp1;
                data[7] = temp0;
            }
            return BitConverter.ToDouble(data, 0);
        }

        internal static EventData DeserializeEventData(MemoryStream din)
        {
            EventData result = new EventData();
            result.Code = DeserializeByte(din);
            result.Parameters = DeserializeParameterTable(din);
            return result;
        }

        private static float DeserializeFloat(MemoryStream din)
        {
            byte[] data = new byte[4];
            din.Read(data, 0, 4);
            if (BitConverter.IsLittleEndian)
            {
                byte temp0 = data[0];
                byte temp1 = data[1];
                data[0] = data[3];
                data[1] = data[2];
                data[2] = temp1;
                data[3] = temp0;
            }
            return BitConverter.ToSingle(data, 0);
        }

        private static Hashtable DeserializeHashTable(MemoryStream din)
        {
            int size = DeserializeShort(din);
            Hashtable value = new Hashtable(size);
            for (int i = 0; i < size; i++)
            {
                object serKey = Deserialize(din, (byte)din.ReadByte());
                object serValue = Deserialize(din, (byte)din.ReadByte());
                value[serKey] = serValue;
            }
            return value;
        }

        private static int[] DeserializeIntArray(MemoryStream din)
        {
            int size = DeserializeInteger(din);
            int[] retVal = new int[size];
            for (int i = 0; i < size; i++)
            {
                retVal[i] = DeserializeInteger(din);
            }
            return retVal;
        }

        private static long DeserializeLong(MemoryStream din)
        {
            byte[] data = new byte[8];
            din.Read(data, 0, 8);
            if (BitConverter.IsLittleEndian)
            {
                return (long)((((((((data[0] << 0x38) | (data[1] << 0x30)) | (data[2] << 40)) | (data[3] << 0x20)) | (data[4] << 0x18)) | (data[5] << 0x10)) | (data[6] << 8)) | data[7]);
            }
            return BitConverter.ToInt64(data, 0);
        }

        private static object[] DeserializeObjectArray(MemoryStream din)
        {
            short arrayLength = DeserializeShort(din);
            object[] resultArray = new object[arrayLength];
            for (int i = 0; i < arrayLength; i++)
            {
                byte typeCode = (byte)din.ReadByte();
                resultArray.SetValue(Deserialize(din, typeCode), i);
            }
            return resultArray;
        }

        internal static OperationRequest DeserializeOperationRequest(MemoryStream din)
        {
            OperationRequest request = new OperationRequest();
            request.OperationCode = DeserializeByte(din);
            request.Parameters = DeserializeParameterTable(din);
            return request;
        }

        internal static OperationResponse DeserializeOperationResponse(MemoryStream memoryStream)
        {
            OperationResponse response = new OperationResponse();
            response.OperationCode = DeserializeByte(memoryStream);
            response.ReturnCode = DeserializeShort(memoryStream);
            response.DebugMessage = Deserialize(memoryStream, DeserializeByte(memoryStream)) as string;
            response.Parameters = DeserializeParameterTable(memoryStream);
            return response;
        }

        private static Dictionary<byte, object> DeserializeParameterTable(MemoryStream memoryStream)
        {
            short numRetVals = DeserializeShort(memoryStream);
            Dictionary<byte, object> retVals = new Dictionary<byte, object>(numRetVals);
            for (int i = 0; i < numRetVals; i++)
            {
                byte keyByteCode = (byte)memoryStream.ReadByte();
                object valueObject = Deserialize(memoryStream, (byte)memoryStream.ReadByte());
                retVals[keyByteCode] = valueObject;
            }
            return retVals;
        }

        private static short DeserializeShort(MemoryStream din)
        {
            byte[] data = new byte[2];
            din.Read(data, 0, 2);
            return (short)((data[0] << 8) | data[1]);
        }

        private static string DeserializeString(MemoryStream din)
        {
            short length = DeserializeShort(din);
            if (length == 0)
            {
                return "";
            }
            byte[] Read = new byte[length];
            din.Read(Read, 0, Read.Length);
            return Encoding.UTF8.GetString(Read, 0, Read.Length);
        }

        private static string[] DeserializeStringArray(MemoryStream din)
        {
            int size = DeserializeShort(din);
            string[] val = new string[size];
            for (int i = 0; i < size; i++)
            {
                val[i] = DeserializeString(din);
            }
            return val;
        }

        private static ArrayList DeserializeVector(MemoryStream din)
        {
            int size = DeserializeShort(din);
            ArrayList val = new ArrayList(size);
            if (size > 0)
            {
                byte type = (byte)din.ReadByte();
                for (int i = 0; i < size; i++)
                {
                    val.Add(Deserialize(din, type));
                }
            }
            return val;
        }

        private static byte GetCodeOfType(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                    return (byte)GpType.Boolean;

                case TypeCode.Byte:
                    return (byte)GpType.Byte;

                case TypeCode.Int16:
                    return (byte)GpType.Short;

                case TypeCode.Int32:
                    return (byte)GpType.Integer;

                case TypeCode.Int64:
                    return (byte)GpType.Long;

                case TypeCode.Single:
                    return (byte)GpType.Float;

                case TypeCode.Double:
                    return (byte)GpType.Double;

                case TypeCode.String:
                    return (byte)GpType.String;
            }
            if (type.IsArray)
            {
                if (type == typeof(byte[]))
                {
                    return (byte)GpType.ByteArray;
                }
                return (byte)GpType.Array;
            }
            if (type == typeof(Hashtable))
            {
                return (byte)GpType.Hashtable;
            }
            if (type.IsGenericType && (typeof(Dictionary<,>) == type.GetGenericTypeDefinition()))
            {
                return (byte)GpType.Dictionary;
            }
            if (type == typeof(EventData))
            {
                return (byte)GpType.EventData;
            }
            if (type == typeof(OperationRequest))
            {
                return (byte)GpType.OperationRequest;
            }
            if (type == typeof(OperationResponse))
            {
                return (byte)GpType.OperationResponse;
            }
            return (byte)GpType.Unknown;
        }

        private static Type GetTypeOfCode(byte typeCode)
        {
            switch (typeCode)
            {
                case (byte)GpType.StringArray:
                    return typeof(string[]);

                case (byte)GpType.Byte:
                    return typeof(byte);

                case (byte)GpType.Custom:
                    return typeof(CustomType);

                case (byte)GpType.Double:
                    return typeof(double);

                case (byte)GpType.EventData:
                    return typeof(EventData);

                case (byte)GpType.Float:
                    return typeof(float);

                case (byte)GpType.Hashtable:
                    return typeof(Hashtable);

                case (byte)GpType.Integer:
                    return typeof(int);

                case (byte)GpType.Short:
                    return typeof(short);

                case (byte)GpType.Long:
                    return typeof(long);

                case (byte)GpType.IntegerArray:
                    return typeof(int[]);

                case (byte)GpType.Boolean:
                    return typeof(bool);

                case (byte)GpType.OperationResponse:
                    return typeof(OperationResponse);

                case (byte)GpType.OperationRequest:
                    return typeof(OperationRequest);

                case (byte)GpType.String:
                    return typeof(string);

                case (byte)GpType.ByteArray:
                    return typeof(byte[]);

                case (byte)GpType.Array:
                    return typeof(Array);

                case (byte)GpType.ObjectArray:
                    return typeof(object[]);

                case (byte)GpType.Dictionary:
                    return typeof(IDictionary);

                case (byte)GpType.Unknown:
                case (byte)GpType.Null:
                    return typeof(object);
            }
            Debug.WriteLine("missing type: " + typeCode);
            throw new Exception("deserialize(): " + typeCode);
        }

        private static void SerializeArray(MemoryStream dout, Array serObject, bool setType)
        {
            int index;
            if (setType)
            {
                dout.WriteByte((byte)GpType.Array);
            }
            SerializeShort(dout, (short)serObject.Length, false);
            Type elementType = serObject.GetType().GetElementType();
            byte contentTypeCode = GetCodeOfType(elementType);
            if (contentTypeCode != 0)
            {
                dout.WriteByte(contentTypeCode);
                if (contentTypeCode == (byte)GpType.Dictionary)
                {
                    bool setKeyType;
                    bool setValueType;
                    SerializeDictionaryHeader(dout, serObject, out setKeyType, out setValueType);
                    for (index = 0; index < serObject.Length; index++)
                    {
                        object element = serObject.GetValue(index);
                        SerializeDictionaryElements(dout, element, setKeyType, setValueType);
                    }
                }
                else
                {
                    index = 0;
                    while (index < serObject.Length)
                    {
                        object o = serObject.GetValue(index);
                        Serialize(dout, o, false);
                        index++;
                    }
                }
            }
            else
            {
                CustomType customType;
                if (!TypeDict.TryGetValue(elementType, out customType))
                {
                    throw new NotSupportedException("cannot serialize array of type " + elementType);
                }
                dout.WriteByte((byte)GpType.Custom);
                dout.WriteByte(customType.Code);
                for (index = 0; index < serObject.Length; index++)
                {
                    object obj = serObject.GetValue(index);
                    byte[] custom = customType.SerializeFunction(obj);
                    SerializeShort(dout, (short)custom.Length, false);
                    dout.Write(custom, 0, custom.Length);
                }
            }
        }

        private static void SerializeBoolean(MemoryStream dout, bool serObject, bool setType)
        {
            if (setType)
            {
                dout.WriteByte((byte)GpType.Boolean);
            }
            dout.Write(BitConverter.GetBytes(serObject), 0, 1);
        }

        private static void SerializeByte(MemoryStream dout, byte serObject, bool setType)
        {
            if (setType)
            {
                dout.WriteByte((byte)GpType.Byte);
            }
            dout.WriteByte(serObject);
        }

        private static void SerializeByteArray(MemoryStream dout, byte[] serObject, bool setType)
        {
            if (setType)
            {
                dout.WriteByte((byte)GpType.ByteArray);
            }
            SerializeInteger(dout, serObject.Length, false);
            dout.Write(serObject, 0, serObject.Length);
        }

        private static bool SerializeCustom(MemoryStream dout, object serObject)
        {
            CustomType customType;
            if (TypeDict.TryGetValue(serObject.GetType(), out customType))
            {
                byte[] bytesOfCustomType = customType.SerializeFunction(serObject);
                dout.WriteByte((byte)GpType.Custom);
                dout.WriteByte(customType.Code);
                SerializeShort(dout, (short)bytesOfCustomType.Length, false);
                dout.Write(bytesOfCustomType, 0, bytesOfCustomType.Length);
                return true;
            }
            return false;
        }

        private static void SerializeDictionary(MemoryStream dout, IDictionary serObject, bool setType)
        {
            bool setKeyType;
            bool setValueType;
            if (setType)
            {
                dout.WriteByte((byte)GpType.Dictionary);
            }
            SerializeDictionaryHeader(dout, serObject, out setKeyType, out setValueType);
            SerializeDictionaryElements(dout, serObject, setKeyType, setValueType);
        }

        private static void SerializeDictionaryElements(MemoryStream writer, object dict, bool setKeyType, bool setValueType)
        {
            IDictionary d = (IDictionary)dict;
            SerializeShort(writer, (short)d.Count, false);
            foreach (DictionaryEntry entry in d)
            {
                Serialize(writer, entry.Key, setKeyType);
                Serialize(writer, entry.Value, setValueType);
            }
        }

        private static void SerializeDictionaryHeader(MemoryStream writer, Type dictType)
        {
            bool setKeyType;
            bool setValueType;
            SerializeDictionaryHeader(writer, dictType, out setKeyType, out setValueType);
        }

        private static void SerializeDictionaryHeader(MemoryStream writer, object dict, out bool setKeyType, out bool setValueType)
        {
            Type[] types = dict.GetType().GetGenericArguments();
            setKeyType = types[0] == typeof(object);
            setValueType = types[1] == typeof(object);
            if (setKeyType)
            {
                writer.WriteByte(0);
            }
            else
            {
                GpType keyType = (GpType)GetCodeOfType(types[0]);
                switch (keyType)
                {
                    case GpType.Unknown:
                    case GpType.Dictionary:
                        throw new Exception("Unexpected - cannot serialize Dictionary with key type: " + types[0]);
                }
                writer.WriteByte((byte)keyType);
            }
            if (setValueType)
            {
                writer.WriteByte(0);
            }
            else
            {
                GpType valueType = (GpType)GetCodeOfType(types[1]);
                if (valueType == GpType.Unknown)
                {
                    throw new Exception("Unexpected - cannot serialize Dictionary with value type: " + types[0]);
                }
                writer.WriteByte((byte)valueType);
                if (valueType == GpType.Dictionary)
                {
                    SerializeDictionaryHeader(writer, types[1]);
                }
            }
        }

        private static void SerializeDouble(MemoryStream dout, double serObject, bool setType)
        {
            if (setType)
            {
                dout.WriteByte((byte)GpType.Double);
            }
            byte[] data = BitConverter.GetBytes(serObject);
            if (BitConverter.IsLittleEndian)
            {
                byte temp0 = data[0];
                byte temp1 = data[1];
                byte temp2 = data[2];
                byte temp3 = data[3];
                data[0] = data[7];
                data[1] = data[6];
                data[2] = data[5];
                data[3] = data[4];
                data[4] = temp3;
                data[5] = temp2;
                data[6] = temp1;
                data[7] = temp0;
            }
            dout.Write(data, 0, 8);
        }

        internal static void SerializeEventData(MemoryStream memStream, EventData serObject, bool setType)
        {
            if (setType)
            {
                memStream.WriteByte((byte)GpType.EventData);
            }
            memStream.WriteByte(serObject.Code);
            SerializeParameterTable(memStream, serObject.Parameters);
        }

        private static void SerializeFloat(MemoryStream dout, float serObject, bool setType)
        {
            if (setType)
            {
                dout.WriteByte((byte)GpType.Float);
            }
            byte[] data = BitConverter.GetBytes(serObject);
            if (BitConverter.IsLittleEndian)
            {
                byte temp0 = data[0];
                byte temp1 = data[1];
                data[0] = data[3];
                data[1] = data[2];
                data[2] = temp1;
                data[3] = temp0;
            }
            dout.Write(data, 0, 4);
        }

        private static void SerializeHashTable(MemoryStream dout, Hashtable serObject, bool setType)
        {
            if (setType)
            {
                dout.WriteByte((byte)GpType.Hashtable);
            }
            SerializeShort(dout, (short)serObject.Count, false);
            foreach (DictionaryEntry entry in serObject)
            {
                Serialize(dout, entry.Key, true);
                Serialize(dout, entry.Value, true);
            }
        }

        private static void SerializeIntArrayOptimized(MemoryStream inWriter, int[] serObject, bool setType)
        {
            if (setType)
            {
                inWriter.WriteByte((byte)GpType.Array);
            }
            SerializeShort(inWriter, (short)serObject.Length, false);
            inWriter.WriteByte((byte)GpType.Integer);
            byte[] temp = new byte[serObject.Length * 4];
            int x = 0;
            for (int i = 0; i < serObject.Length; i++)
            {
                temp[x++] = (byte)(serObject[i] >> 0x18);
                temp[x++] = (byte)(serObject[i] >> 0x10);
                temp[x++] = (byte)(serObject[i] >> 8);
                temp[x++] = (byte)serObject[i];
            }
            inWriter.Write(temp, 0, temp.Length);
        }

        private static void SerializeInteger(MemoryStream dout, int serObject, bool setType)
        {
            byte[] buff = new byte[] { (byte)GpType.Integer, (byte)(serObject >> 0x18), (byte)(serObject >> 0x10), (byte)(serObject >> 8), (byte)serObject };
            dout.Write(buff, setType ? 0 : 1, setType ? 5 : 4);
        }

        private static void SerializeLong(MemoryStream dout, long serObject, bool setType)
        {
            if (setType)
            {
                dout.WriteByte((byte)GpType.Long);
            }
            byte[] data = BitConverter.GetBytes(serObject);
            if (BitConverter.IsLittleEndian)
            {
                byte temp0 = data[0];
                byte temp1 = data[1];
                byte temp2 = data[2];
                byte temp3 = data[3];
                data[0] = data[7];
                data[1] = data[6];
                data[2] = data[5];
                data[3] = data[4];
                data[4] = temp3;
                data[5] = temp2;
                data[6] = temp1;
                data[7] = temp0;
            }
            dout.Write(data, 0, 8);
        }

        private static void SerializeObjectArray(MemoryStream dout, object[] objects, bool setType)
        {
            if (setType)
            {
                dout.WriteByte((byte)GpType.ObjectArray);
            }
            SerializeShort(dout, (short)objects.Length, false);
            for (int index = 0; index < objects.Length; index++)
            {
                object obj = objects[index];
                Serialize(dout, obj, true);
            }
        }

        internal static void SerializeOperationRequest(MemoryStream memStream, OperationRequest serObject, bool setType)
        {
            SerializeOperationRequest(memStream, serObject.OperationCode, serObject.Parameters, setType);
        }

        internal static void SerializeOperationRequest(MemoryStream memStream, byte operationCode, Dictionary<byte, object> parameters, bool setType)
        {
            if (setType)
            {
                memStream.WriteByte((byte)GpType.OperationRequest);
            }
            memStream.WriteByte(operationCode);
            SerializeParameterTable(memStream, parameters);
        }

        internal static void SerializeOperationResponse(MemoryStream memStream, OperationResponse serObject, bool setType)
        {
            if (setType)
            {
                memStream.WriteByte((byte)GpType.OperationResponse);
            }
            memStream.WriteByte(serObject.OperationCode);
            SerializeShort(memStream, serObject.ReturnCode, false);
            if (string.IsNullOrEmpty(serObject.DebugMessage))
            {
                memStream.WriteByte((byte)GpType.Null);
            }
            else
            {
                SerializeString(memStream, serObject.DebugMessage, false);
            }
            SerializeParameterTable(memStream, serObject.Parameters);
        }

        private static void SerializeParameterTable(MemoryStream memStream, Dictionary<byte, object> parameters)
        {
            if ((parameters == null) || (parameters.Count == 0))
            {
                SerializeShort(memStream, 0, false);
            }
            else
            {
                SerializeShort(memStream, (short)parameters.Count, false);
                foreach (KeyValuePair<byte, object> pair in parameters)
                {
                    memStream.WriteByte(pair.Key);
                    Serialize(memStream, pair.Value, true);
                }
            }
        }

        private static void SerializeShort(MemoryStream dout, short serObject, bool setType)
        {
            if (setType)
            {
                dout.WriteByte((byte)GpType.Short);
            }
            byte[] temp = new byte[] { (byte)(serObject >> 8), (byte)serObject };
            dout.Write(temp, 0, 2);
        }

        private static void SerializeString(MemoryStream dout, string serObject, bool setType)
        {
            if (setType)
            {
                dout.WriteByte((byte)GpType.String);
            }
            byte[] Write = Encoding.UTF8.GetBytes(serObject);
            SerializeShort(dout, (short)Write.Length, false);
            dout.Write(Write, 0, Write.Length);
        }

        private static void SerializeStringArray(MemoryStream dout, string[] serObject, bool setType)
        {
            if (setType)
            {
                dout.WriteByte((byte)GpType.StringArray);
            }
            SerializeShort(dout, (short)serObject.Length, false);
            for (int i = 0; i < serObject.Length; i++)
            {
                SerializeString(dout, serObject[i], false);
            }
        }

        private static void SerializeVector(MemoryStream dout, ArrayList serObject, bool setType)
        {
            if (setType)
            {
                dout.WriteByte((byte)GpType.Vector);
            }
            SerializeShort(dout, (short)serObject.Count, false);
            bool first = true;
            IEnumerator e = serObject.GetEnumerator();
            while (e.MoveNext())
            {
                Serialize(dout, e.Current, first);
                first = false;
            }
        }

        internal static bool TryRegisterType(Type type, byte typeCode, SerializeMethod serializeFunction, DeserializeMethod deserializeFunction)
        {
            if (CodeDict.ContainsKey(typeCode) || TypeDict.ContainsKey(type))
            {
                return false;
            }
            CustomType customType = new CustomType(type, typeCode, serializeFunction, deserializeFunction);
            CodeDict.Add(typeCode, customType);
            TypeDict.Add(type, customType);
            return true;
        }
    }
}
