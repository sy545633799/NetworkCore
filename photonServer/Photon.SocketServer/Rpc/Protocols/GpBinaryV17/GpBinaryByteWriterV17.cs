using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Photon.SocketServer.Rpc.ValueTypes;

namespace Photon.SocketServer.Rpc.Protocols.GpBinaryV17
{
    internal static class GpBinaryByteWriterV17
    {
        // Fields
        private static readonly WriteDelegate[] writeMethods = new WriteDelegate[0x100];

        // Methods
        static GpBinaryByteWriterV17()
        {
            writeMethods[0x6f] = new WriteDelegate(GpBinaryByteWriterV17.WriteBoolean);
            writeMethods[0x62] = new WriteDelegate(GpBinaryByteWriterV17.WriteByte);
            writeMethods[0x6b] = new WriteDelegate(GpBinaryByteWriterV17.WriteInt16);
            writeMethods[0x69] = new WriteDelegate(GpBinaryByteWriterV17.WriteCompressedInt32);
            writeMethods[0x6c] = new WriteDelegate(GpBinaryByteWriterV17.WriteCompressedInt64);
            writeMethods[3] = new WriteDelegate(GpBinaryByteWriterV17.WriteCompressedInt32);
            writeMethods[4] = new WriteDelegate(GpBinaryByteWriterV17.WriteCompressedInt64);
            writeMethods[0x66] = new WriteDelegate(GpBinaryByteWriterV17.WriteSingle);
            writeMethods[100] = new WriteDelegate(GpBinaryByteWriterV17.WriteDouble);
            writeMethods[0x73] = new WriteDelegate(GpBinaryByteWriterV17.WriteString);
            writeMethods[0x68] = new WriteDelegate(GpBinaryByteWriterV17.WriteHashTable);
            writeMethods[0x44] = new WriteDelegate(GpBinaryByteWriterV17.WriteDictionary);
            writeMethods[0x65] = new WriteDelegate(GpBinaryByteWriterV17.WriteEventData);
            writeMethods[0xeb] = new WriteDelegate(GpBinaryByteWriterV17.WriteInt16Array);
        }

        private static uint EncodeZigZag32(int value)
        {
            return (uint)((value << 1) ^ (value >> 0x1f));
        }

        private static ulong EncodeZigZag64(long value)
        {
            return (ulong)((value << 1) ^ (value >> 0x3f));
        }

        private static void GetDictionaryDelegates(Type type, out WriteDelegate keyWriteDelegate, out WriteDelegate valueWriteDelegate)
        {
            Type[] genericArguments = type.GetGenericArguments();
            if (genericArguments[0] == typeof(object))
            {
                keyWriteDelegate = new WriteDelegate(GpBinaryByteWriterV17.Write);
            }
            else
            {
                if (!genericArguments[0].IsPrimitive && (genericArguments[0] != typeof(string)))
                {
                    throw new InvalidDataException("Unexpected - cannot serialize Dictionary with key type: " + genericArguments[0]);
                }
                GpTypeV17 gpType = GetGpType(genericArguments[0]);
                keyWriteDelegate = writeMethods[(int)gpType];
                if (keyWriteDelegate == null)
                {
                    throw new InvalidDataException("Unexpected - cannot serialize Dictionary with key type: " + genericArguments[0]);
                }
            }
            if (genericArguments[1] == typeof(object))
            {
                valueWriteDelegate = new WriteDelegate(GpBinaryByteWriterV17.Write);
            }
            else
            {
                GpTypeV17 ev2 = GetGpType(genericArguments[1]);
                valueWriteDelegate = writeMethods[(int)ev2];
                if (valueWriteDelegate == null)
                {
                    throw new InvalidDataException("Unexpected - cannot serialize Dictionary with value type: " + genericArguments[1]);
                }
                if (ev2 == GpTypeV17.Dictionary)
                {
                    valueWriteDelegate = new WriteDelegate(GpBinaryByteWriterV17.WriteDictionary2);
                }
            }
        }

        private static GpTypeV17 GetGpType(Type type)
        {
            if (type.IsPrimitive)
            {
                return GetGpType(Type.GetTypeCode(type));
            }
            if (type == typeof(string))
            {
                return GpTypeV17.String;
            }
            if (type == typeof(Hashtable))
            {
                return GpTypeV17.Hashtable;
            }
            if (type.IsGenericType)
            {
                Type genericTypeDefinition = type.GetGenericTypeDefinition();
                if (typeof(Dictionary<,>) == genericTypeDefinition)
                {
                    return GpTypeV17.Dictionary;
                }
            }
            if (type == typeof(RawCustomValue))
            {
                return GpTypeV17.Custom;
            }
            if (typeof(IEventData).IsAssignableFrom(type))
            {
                return GpTypeV17.EventData;
            }
            if (type == typeof(OperationResponse))
            {
                return GpTypeV17.OperationResponse;
            }
            if (type == typeof(OperationRequest))
            {
                return GpTypeV17.OperationRequest;
            }
            return GpTypeV17.Unknown;
        }

        private static GpTypeV17 GetGpType(TypeCode typeCode)
        {
            switch (typeCode)
            {
                case TypeCode.Boolean:
                    return GpTypeV17.Boolean;

                case TypeCode.Byte:
                    return GpTypeV17.Byte;

                case TypeCode.Int16:
                    return GpTypeV17.Short;

                case TypeCode.Int32:
                    return GpTypeV17.CompressedInt;

                case TypeCode.Int64:
                    return GpTypeV17.CompressedLong;

                case TypeCode.Single:
                    return GpTypeV17.Float;

                case TypeCode.Double:
                    return GpTypeV17.Double;

                case TypeCode.String:
                    return GpTypeV17.String;
            }
            return GpTypeV17.Unknown;
        }

        public static void Write(Stream stream, object value)
        {
            if (value == null)
            {
                stream.WriteByte(0x2a);
            }
            else
            {
                Type type = value.GetType();
                if (type.IsArray)
                {
                    WriteArray(stream, type, value);
                }
                else
                {
                    GpTypeV17 gpType = GetGpType(type);
                    WriteDelegate delegate2 = writeMethods[(int)gpType];
                    if (delegate2 != null)
                    {
                        stream.WriteByte((byte)gpType);
                        delegate2(stream, value);
                    }
                    else
                    {
                        CustomTypeInfo info;
                        if (CustomTypeCache.TryGet(type, out info))
                        {
                            WriteCustomType(stream, info, value, true);
                        }
                        else if (type == typeof(RawCustomValue))
                        {
                            WriteCustomType(stream, (RawCustomValue)value, true);
                        }
                        else
                        {
                            if (type != typeof(RawCustomArray))
                            {
                                throw new InvalidDataException("cannot serialize(): " + type);
                            }
                            WriteCustomTypeArray(stream, (RawCustomArray)value, true);
                        }
                    }
                }
            }
        }

        private static void WriteArray(Stream stream, Type type, object value)
        {
            Type elementType = type.GetElementType();
            if (elementType == null)
            {
                throw new InvalidDataException(string.Format("Arraysof type {0} are not supported", type));
            }
            if (elementType.IsPrimitive)
            {
                switch (Type.GetTypeCode(elementType))
                {
                    case TypeCode.Boolean:
                        WriteBoolArray(stream, (bool[])value, true);
                        return;

                    case TypeCode.Byte:
                        stream.WriteByte(120);
                        WriteByteArray(stream, (byte[])value);
                        return;

                    case TypeCode.Int16:
                        stream.WriteByte(0xeb);
                        WriteInt16Array(stream, (short[])value);
                        return;

                    case TypeCode.Int32:
                        WriteInt32ArrayCompressed(stream, (int[])value, true);
                        return;

                    case TypeCode.Int64:
                        WriteInt64ArrayCompressed(stream, (long[])value, true);
                        return;

                    case TypeCode.Single:
                        WriteSingleArray(stream, (float[])value, true);
                        return;

                    case TypeCode.Double:
                        WriteDoubleArray(stream, (double[])value, true);
                        return;
                }
            }
            if (elementType.IsArray)
            {
                WriteArrayInArray(stream, value);
            }
            if (elementType == typeof(string))
            {
                WriteStringArray(stream, (string[])value, true);
            }
            else if (elementType == typeof(object))
            {
                stream.WriteByte(0x7a);
                WriteObjectArray(stream, (object[])value);
            }
            else if (elementType == typeof(Hashtable))
            {
                Hashtable[] hashtableArray = (Hashtable[])value;
                stream.WriteByte(0xe8);
                WriteIntLength(stream, hashtableArray.Length);
                foreach (Hashtable hashtable in hashtableArray)
                {
                    WriteHashTable(stream, hashtable);
                }
            }
            else
            {
                CustomTypeInfo info;
                if (elementType.IsGenericType)
                {
                    Type genericTypeDefinition = elementType.GetGenericTypeDefinition();
                    if (typeof(Dictionary<,>) == genericTypeDefinition)
                    {
                        WriteDelegate delegate2;
                        WriteDelegate delegate3;
                        IDictionary[] dictionaryArray = (IDictionary[])value;
                        stream.WriteByte(0xc4);
                        WriteDictionaryHeader(stream, elementType, out delegate2, out delegate3);
                        WriteIntLength(stream, dictionaryArray.Length);
                        foreach (IDictionary dictionary in dictionaryArray)
                        {
                            WriteDictionaryElements(stream, dictionary, delegate2, delegate3);
                        }
                    }
                }
                if (CustomTypeCache.TryGet(elementType, out info))
                {
                    WriteCustomTypeArray(stream, info, (IList)value);
                }
            }
        }

        private static bool WriteArrayHeader(Stream stream, Type type)
        {
            Type elementType = type.GetElementType();
            while (elementType.IsArray)
            {
                stream.WriteByte(0x79);
                elementType = elementType.GetElementType();
            }
            GpTypeV17 gpType = GetGpType(elementType);
            if (gpType == GpTypeV17.Unknown)
            {
                return false;
            }
            stream.WriteByte((byte)(gpType | ((GpTypeV17)0x80)));
            return true;
        }

        private static void WriteArrayInArray(Stream stream, object value)
        {
            object[] objArray = (object[])value;
            stream.WriteByte(0x79);
            WriteIntLength(stream, objArray.Length);
            foreach (object obj2 in objArray)
            {
                Write(stream, obj2);
            }
        }

        private static bool WriteArrayType(Stream stream, Type type, out WriteDelegate writeDelegate)
        {
            Type elementType = type.GetElementType();
            if (elementType == null)
            {
                throw new InvalidDataException("Unexpected - cannot serialize array with type: " + type);
            }
            if (elementType.IsArray)
            {
                while ((elementType != null) && elementType.IsArray)
                {
                    stream.WriteByte(0x79);
                    elementType = elementType.GetElementType();
                }
                byte num = (byte)(GetGpType(Type.GetTypeCode(elementType)) | ((GpTypeV17)0x80));
                stream.WriteByte(num);
                writeDelegate = new WriteDelegate(GpBinaryByteWriterV17.WriteArrayInArray);
                return true;
            }
            if (elementType.IsPrimitive)
            {
                byte num2 = (byte)(GetGpType(Type.GetTypeCode(elementType)) | ((GpTypeV17)0x80));
                stream.WriteByte(num2);
                writeDelegate = writeMethods[num2];
                return (writeDelegate != null);
            }
            if (elementType == typeof(string))
            {
                stream.WriteByte(0xf3);
                writeDelegate = new WriteDelegate(GpBinaryByteWriterV17.WriteStringArray);
                return true;
            }
            writeDelegate = null;
            return false;
        }

        internal static void WriteBoolArray(Stream stream, bool[] value, bool writeType)
        {
            int num3;
            int num = value.Length >> 3;
            uint num2 = (uint)(8 + num);
            byte[] buffer = new byte[num2];
            if (writeType)
            {
                buffer[0] = 0xef;
                num3 = 1;
            }
            else
            {
                num3 = 0;
            }
            WriteCompressedUInt32(buffer, (uint)value.Length, ref num3);
            int index = 0;
            while (num > 0)
            {
                byte num5 = 0;
                if (value[index++])
                {
                    num5 = (byte)(num5 | 1);
                }
                if (value[index++])
                {
                    num5 = (byte)(num5 | 2);
                }
                if (value[index++])
                {
                    num5 = (byte)(num5 | 4);
                }
                if (value[index++])
                {
                    num5 = (byte)(num5 | 8);
                }
                if (value[index++])
                {
                    num5 = (byte)(num5 | 0x10);
                }
                if (value[index++])
                {
                    num5 = (byte)(num5 | 0x20);
                }
                if (value[index++])
                {
                    num5 = (byte)(num5 | 0x40);
                }
                if (value[index++])
                {
                    num5 = (byte)(num5 | 0x80);
                }
                buffer[num3] = num5;
                num--;
                num3++;
            }
            if (index < value.Length)
            {
                byte num6 = 0;
                int num7 = 0;
                while (index < value.Length)
                {
                    if (value[index])
                    {
                        num6 = (byte)(num6 | ((byte)(((int)1) << num7)));
                    }
                    num7++;
                    index++;
                }
                buffer[num3] = num6;
                num3++;
            }
            stream.Write(buffer, 0, num3);
        }

        private static void WriteBoolean(Stream stream, object value)
        {
            stream.WriteByte(((bool)value) ? ((byte)1) : ((byte)0));
        }

        private static void WriteByte(Stream stream, object value)
        {
            stream.WriteByte((byte)value);
        }

        private static void WriteByteArray(Stream stream, byte[] value)
        {
            WriteIntLength(stream, value.Length);
            stream.Write(value, 0, value.Length);
        }

        public static void WriteCompressedInt32(Stream stream, object value)
        {
            uint val = EncodeZigZag32((int)value);
            WriteCompressedUInt32(stream, val);
        }

        private static void WriteCompressedInt32(byte[] buffer, int value, ref int position)
        {
            uint num = EncodeZigZag32(value);
            WriteCompressedUInt32(buffer, num, ref position);
        }

        public static void WriteCompressedInt64(Stream stream, object value)
        {
            ulong val = EncodeZigZag64((long)value);
            WriteCompressedUInt64(stream, val);
        }

        private static void WriteCompressedInt64(byte[] buffer, long value, ref int position)
        {
            ulong val = EncodeZigZag64(value);
            WriteCompressedUInt64(buffer, val, ref position);
        }

        internal static void WriteCompressedUInt32(Stream stream, uint val)
        {
            byte[] buffer = new byte[5];
            int index = 0;
            buffer[index] = (byte)(val & 0x7f);
            val = val >> 7;
            while (val > 0)
            {
                buffer[index] = (byte)(buffer[index] | 0x80);
                buffer[++index] = (byte)(val & 0x7f);
                val = val >> 7;
            }
            stream.Write(buffer, 0, index + 1);
        }

        private static void WriteCompressedUInt32(byte[] buffer, uint value, ref int position)
        {
            buffer[position] = (byte)(value & 0x7f);
            value = value >> 7;
            while (value > 0)
            {
                int num;
                buffer[position] = (byte)(buffer[position] | 0x80);
                position = num = position + 1;
                buffer[num] = (byte)(value & 0x7f);
                value = value >> 7;
            }
            position++;
        }

        internal static void WriteCompressedUInt64(Stream stream, ulong val)
        {
            byte[] buffer = new byte[10];
            int index = 0;
            buffer[index] = (byte)(val & ((ulong)0x7fL));
            val = val >> 7;
            while (val > 0L)
            {
                buffer[index] = (byte)(buffer[index] | 0x80);
                buffer[++index] = (byte)(val & ((ulong)0x7fL));
                val = val >> 7;
            }
            stream.Write(buffer, 0, index + 1);
        }

        internal static void WriteCompressedUInt64(byte[] buffer, ulong val, ref int pos)
        {
            buffer[pos] = (byte)(val & ((ulong)0x7fL));
            val = val >> 7;
            while (val > 0L)
            {
                int num;
                buffer[pos] = (byte)(buffer[pos] | 0x80);
                pos = num = pos + 1;
                buffer[num] = (byte)(val & ((ulong)0x7fL));
                val = val >> 7;
            }
            pos++;
        }

        private static void WriteCustomType(Stream stream, RawCustomValue customType, bool writeType)
        {
            if (writeType)
            {
                stream.WriteByte(0x63);
            }
            stream.WriteByte(customType.Code);
            WriteIntLength(stream, customType.Data.Length);
            stream.Write(customType.Data, 0, customType.Data.Length);
        }

        private static void WriteCustomType(Stream stream, CustomTypeInfo customTypeInfo, object value, bool writeType)
        {
            if (writeType)
            {
                stream.WriteByte(0x63);
            }
            stream.WriteByte(customTypeInfo.Code);
            byte[] buffer = customTypeInfo.SerializeFunction(value);
            WriteIntLength(stream, buffer.Length);
            stream.Write(buffer, 0, buffer.Length);
        }

        private static void WriteCustomTypeArray(Stream stream, CustomTypeInfo customTypeInfo, IList list)
        {
            stream.WriteByte(0xe3);
            WriteIntLength(stream, (short)list.Count);
            stream.WriteByte(customTypeInfo.Code);
            foreach (object obj2 in list)
            {
                byte[] buffer = customTypeInfo.SerializeFunction(obj2);
                WriteIntLength(stream, buffer.Length);
                stream.Write(buffer, 0, buffer.Length);
            }
        }

        private static void WriteCustomTypeArray(Stream stream, RawCustomArray array, [Optional, DefaultParameterValue(true)] bool writeArrayIdentifier)
        {
            if (writeArrayIdentifier)
            {
                stream.WriteByte(0xe3);
            }
            WriteIntLength(stream, array.Length);
            stream.WriteByte(array.Code);
            for (int i = 0; i < array.Length; i++)
            {
                WriteIntLength(stream, array[i].Length);
                stream.Write(array[i], 0, array[i].Length);
            }
        }

        private static void WriteDictionary(Stream stream, object dict)
        {
            WriteDelegate delegate2;
            WriteDelegate delegate3;
            WriteDictionaryHeader(stream, dict.GetType(), out delegate2, out delegate3);
            IDictionary dictionary = (IDictionary)dict;
            WriteDictionaryElements(stream, dictionary, delegate2, delegate3);
        }

        private static void WriteDictionary2(Stream stream, object dict)
        {
            WriteDelegate delegate2;
            WriteDelegate delegate3;
            GetDictionaryDelegates(dict.GetType(), out delegate2, out delegate3);
            IDictionary dictionary = (IDictionary)dict;
            WriteDictionaryElements(stream, dictionary, delegate2, delegate3);
        }

        private static void WriteDictionaryElements(Stream stream, IDictionary dictionary, WriteDelegate keyWriteDelegate, WriteDelegate valueWriteDelegate)
        {
            WriteIntLength(stream, dictionary.Count);
            foreach (DictionaryEntry entry in dictionary)
            {
                keyWriteDelegate(stream, entry.Key);
                valueWriteDelegate(stream, entry.Value);
            }
        }

        private static void WriteDictionaryHeader(Stream stream, Type type, out WriteDelegate keyWriteDelegate, out WriteDelegate valueWriteDelegate)
        {
            Type[] genericArguments = type.GetGenericArguments();
            if (genericArguments[0] == typeof(object))
            {
                stream.WriteByte(0);
                keyWriteDelegate = new WriteDelegate(GpBinaryByteWriterV17.Write);
            }
            else
            {
                if (!genericArguments[0].IsPrimitive && (genericArguments[0] != typeof(string)))
                {
                    throw new InvalidDataException("Unexpected - cannot serialize Dictionary with key type: " + genericArguments[0]);
                }
                GpTypeV17 gpType = GetGpType(genericArguments[0]);
                keyWriteDelegate = writeMethods[(int)gpType];
                if (keyWriteDelegate == null)
                {
                    throw new InvalidDataException("Unexpected - cannot serialize Dictionary with key type: " + genericArguments[0]);
                }
                stream.WriteByte((byte)gpType);
            }
            if (genericArguments[1] == typeof(object))
            {
                stream.WriteByte(0);
                valueWriteDelegate = new WriteDelegate(GpBinaryByteWriterV17.Write);
            }
            else if (genericArguments[1].IsArray)
            {
                if (!WriteArrayType(stream, genericArguments[1], out valueWriteDelegate))
                {
                    throw new InvalidDataException("Unexpected - cannot serialize Dictionary with value type: " + genericArguments[1]);
                }
            }
            else
            {
                GpTypeV17 ev2 = GetGpType(genericArguments[1]);
                valueWriteDelegate = writeMethods[(int)ev2];
                if (valueWriteDelegate == null)
                {
                    throw new InvalidDataException("Unexpected - cannot serialize Dictionary with value type: " + genericArguments[1]);
                }
                switch (ev2)
                {
                    case GpTypeV17.Array:
                        if (!WriteArrayHeader(stream, genericArguments[1]))
                        {
                            throw new InvalidDataException("Unexpected - cannot serialize Dictionary with value type: " + genericArguments[1]);
                        }
                        break;

                    case GpTypeV17.Dictionary:
                        WriteDelegate delegate2;
                        WriteDelegate delegate3;
                        stream.WriteByte((byte)ev2);
                        WriteDictionaryHeader(stream, genericArguments[1], out delegate2, out delegate3);
                        return;

                    default:
                        stream.WriteByte((byte)ev2);
                        break;
                }
            }
        }

        private static unsafe void WriteDouble(Stream stream, double value)
        {
            byte* numPtr = (byte*)&value;
            stream.Write(new byte[] { numPtr[7], numPtr[6], numPtr[5], numPtr[4], numPtr[3], numPtr[2], numPtr[1], numPtr[0] }, 0, 8);
        }

        private static void WriteDouble(Stream stream, object value)
        {
            WriteDouble(stream, (double)value);
        }

        internal static unsafe void WriteDoubleArray(Stream stream, double[] values, bool setType)
        {
            int num;
            byte[] buffer = new byte[7 + (values.Length * 8)];
            if (setType)
            {
                buffer[0] = 0xe4;
                num = 1;
            }
            else
            {
                num = 0;
            }
            WriteCompressedUInt32(buffer, (uint)values.Length, ref num);
            for (int i = 0; i < values.Length; i++)
            {
                double num3 = values[i];
                byte* numPtr = (byte*)&num3;
                buffer[num] = numPtr[7];
                buffer[num + 1] = numPtr[6];
                buffer[num + 2] = numPtr[5];
                buffer[num + 3] = numPtr[4];
                buffer[num + 4] = numPtr[3];
                buffer[num + 5] = numPtr[2];
                buffer[num + 6] = numPtr[1];
                buffer[num + 7] = numPtr[0];
                num += 8;
            }
            stream.Write(buffer, 0, num);
        }

        public static void WriteEventData(Stream stream, IEventData eventData)
        {
            stream.WriteByte(eventData.Code);
            if (eventData.Parameters == null)
            {
                stream.WriteByte(0);
            }
            else
            {
                stream.WriteByte((byte)eventData.Parameters.Count);
                foreach (KeyValuePair<byte, object> pair in eventData.Parameters)
                {
                    stream.WriteByte(pair.Key);
                    Write(stream, pair.Value);
                }
            }
        }

        public static void WriteEventData(Stream stream, object value)
        {
            WriteEventData(stream, (IEventData)value);
        }

        private static void WriteHashTable(Stream stream, object value)
        {
            Hashtable hashtable = (Hashtable)value;
            WriteIntLength(stream, hashtable.Count);
            foreach (DictionaryEntry entry in hashtable)
            {
                Write(stream, entry.Key);
                Write(stream, entry.Value);
            }
        }

        private static void WriteInt16(Stream stream, short value)
        {
            byte[] buffer = new byte[] { (byte)((value >> 8) & 0xff), (byte)(value & 0xff) };
            stream.Write(buffer, 0, 2);
        }

        private static void WriteInt16(Stream stream, object value)
        {
            WriteInt16(stream, (short)value);
        }

        internal static void WriteInt16Array(Stream stream, object value)
        {
            WriteInt16Array(stream, (short[])value);
        }

        internal static unsafe void WriteInt16Array(Stream stream, short[] value)
        {
            byte[] buffer = new byte[7 + (value.Length * 2)];
            int position = 0;
            WriteCompressedUInt32(buffer, (uint)value.Length, ref position);
            for (int i = 0; i < value.Length; i++)
            {
                short num3 = value[i];
                byte* numPtr = (byte*)&num3;
                buffer[position] = numPtr[1];
                buffer[position + 1] = numPtr[0];
                position += 2;
            }
            stream.Write(buffer, 0, position);
        }

        private static void WriteInt32ArrayCompressed(Stream stream, int[] values, bool setType)
        {
            int num2;
            uint num = (uint)(7 + (5 * values.Length));
            byte[] buffer = new byte[num];
            if (setType)
            {
                buffer[0] = 0x83;
                num2 = 1;
            }
            else
            {
                num2 = 0;
            }
            WriteCompressedUInt32(buffer, (uint)values.Length, ref num2);
            for (int i = 0; i < values.Length; i++)
            {
                WriteCompressedInt32(buffer, values[i], ref num2);
            }
            stream.Write(buffer, 0, num2);
        }

        private static void WriteInt64ArrayCompressed(Stream stream, long[] values, bool setType)
        {
            int num2;
            uint num = (uint)(7 + (10 * values.Length));
            byte[] buffer = new byte[num];
            if (setType)
            {
                buffer[0] = 0x84;
                num2 = 1;
            }
            else
            {
                num2 = 0;
            }
            WriteCompressedUInt32(buffer, (uint)values.Length, ref num2);
            for (int i = 0; i < values.Length; i++)
            {
                WriteCompressedInt64(buffer, values[i], ref num2);
            }
            stream.Write(buffer, 0, num2);
        }

        private static void WriteIntLength(Stream stream, int length)
        {
            WriteCompressedUInt32(stream, (uint)length);
        }

        private static void WriteObjectArray(Stream stream, object[] array)
        {
            WriteIntLength(stream, array.Length);
            foreach (object obj2 in array)
            {
                Write(stream, obj2);
            }
        }

        public static void WriteOperationRequest(Stream stream, OperationRequest operationRequest)
        {
            stream.WriteByte(operationRequest.OperationCode);
            if (operationRequest.Parameters != null)
            {
                stream.WriteByte((byte)operationRequest.Parameters.Count);
                foreach (KeyValuePair<byte, object> pair in operationRequest.Parameters)
                {
                    stream.WriteByte(pair.Key);
                    Write(stream, pair.Value);
                }
            }
            else
            {
                stream.WriteByte(0);
            }
        }

        public static void WriteOperationRequest(Stream stream, object value)
        {
            WriteOperationRequest(stream, (OperationRequest)value);
        }

        public static void WriteOperationResponse(Stream stream, OperationResponse operationResponse)
        {
            stream.WriteByte(operationResponse.OperationCode);
            WriteInt16(stream, operationResponse.ReturnCode);
            if (string.IsNullOrEmpty(operationResponse.DebugMessage))
            {
                stream.WriteByte(0x2a);
            }
            else
            {
                stream.WriteByte(0x73);
                WriteString(stream, operationResponse.DebugMessage);
            }
            if (operationResponse.Parameters == null)
            {
                stream.WriteByte(0);
            }
            else
            {
                stream.WriteByte((byte)operationResponse.Parameters.Count);
                foreach (KeyValuePair<byte, object> pair in operationResponse.Parameters)
                {
                    stream.WriteByte(pair.Key);
                    Write(stream, pair.Value);
                }
            }
        }

        public static void WriteOperationResponse(Stream stream, object operationResponse)
        {
            WriteOperationResponse(stream, (OperationResponse)operationResponse);
        }

        private static void WriteSingle(Stream stream, object value)
        {
            WriteSingle(stream, (float)value);
        }

        private static unsafe void WriteSingle(Stream stream, float value)
        {
            byte* numPtr = (byte*)&value;
            stream.Write(new byte[] { numPtr[3], numPtr[2], numPtr[1], numPtr[0] }, 0, 4);
        }

        internal static unsafe void WriteSingleArray(Stream stream, float[] values, bool setType)
        {
            int num;
            byte[] buffer = new byte[7 + (values.Length * 4)];
            if (setType)
            {
                buffer[0] = 230;
                num = 1;
            }
            else
            {
                num = 0;
            }
            WriteCompressedUInt32(buffer, (uint)values.Length, ref num);
            for (int i = 0; i < values.Length; i++)
            {
                float num3 = values[i];
                byte* numPtr = (byte*)&num3;
                buffer[num] = numPtr[3];
                buffer[num + 1] = numPtr[2];
                buffer[num + 2] = numPtr[1];
                buffer[num + 3] = numPtr[0];
                num += 4;
            }
            stream.Write(buffer, 0, num);
        }

        private static void WriteString(Stream stream, object value)
        {
            WriteString(stream, (string)value);
        }

        private static void WriteString(Stream stream, string value)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            WriteCompressedUInt32(stream, (uint)bytes.Length);
            stream.Write(bytes, 0, bytes.Length);
        }

        internal static void WriteStringArray(Stream stream, object value)
        {
            WriteStringArray(stream, (string[])value, false);
        }

        internal static void WriteStringArray(Stream stream, string[] values, bool setType)
        {
            if (setType)
            {
                stream.WriteByte(0xf3);
            }
            WriteCompressedUInt32(stream, (uint)values.Length);
            for (int i = 0; i < values.Length; i++)
            {
                WriteString(stream, values[i]);
            }
        }

        internal static void WriteVarInt32(Stream stream, int value, bool setType)
        {
            if (setType && (value >= 0))
            {
                if (value <= 0xff)
                {
                    stream.Write(new byte[] { 1, (byte)value }, 0, 2);
                    return;
                }
                if (value <= 0xffff)
                {
                    stream.Write(new byte[] { 2, (byte)(value >> 8), (byte)value }, 0, 3);
                    return;
                }
            }
            if (setType)
            {
                stream.WriteByte(3);
            }
            WriteCompressedInt32(stream, value);
        }

        // Nested Types
        private delegate void WriteDelegate(Stream stream, object value);
    }
}
