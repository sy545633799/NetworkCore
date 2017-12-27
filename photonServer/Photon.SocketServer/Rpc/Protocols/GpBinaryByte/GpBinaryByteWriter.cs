using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using ExitGames.IO;
using Photon.SocketServer.Rpc.ValueTypes;

namespace Photon.SocketServer.Rpc.Protocols.GpBinaryByte
{
    /// <summary>
    ///  gp writer.
    /// </summary>
    internal static class GpBinaryByteWriter
    {
        /// <summary>
        /// Serializes an object.
        /// </summary>
        /// <param name="writer"> The writer.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="T:System.IO.InvalidDataException">
        /// The type of the <paramref name="value"/> can not be serialized.
        /// </exception>
        /// <exception cref="T:System.ArrayTypeMismatchException">
        ///   A collection with different types can not be serialized.
        /// </exception>
        public static void Write(IBinaryWriter writer, object value)
        {
            Write(writer, value, true);
        }

        /// <summary>
        /// Serializes an object.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="value">The object to write.</param>
        /// <param name="setType">The set type.</param>
        /// <exception cref="T:System.IO.InvalidDataException">
        /// The type of the <paramref name="value"/> can not be serialized.
        /// </exception>
        /// <exception cref="T:System.ArrayTypeMismatchException">
        /// A collection with different types can not be serialized.
        /// </exception>
        private static void Write(IBinaryWriter writer, object value, bool setType)
        {
            Type type = (value == null) ? null : value.GetType();
            GpType gpType = GpBinaryByteTypeConverter.GetGpType(type);
            GpType type3 = gpType;
            if (type3 != GpType.Unknown)
            {
                if ((type3 == GpType.Array) && (value is byte[]))
                {
                    gpType = GpType.ByteArray;
                }
            }
            else
            {
                CustomTypeInfo info;
                if (type == typeof(RawCustomArray))
                {
                    WriteCustomTypeArray(writer, (RawCustomArray)value, true);
                    return;
                }
                if (CustomTypeCache.TryGet(type, out info))
                {
                    if (setType)
                    {
                        writer.WriteByte(0x63);
                    }
                    WriteCustomType(writer, info, value);
                    return;
                }
                if (type != typeof(RawCustomValue))
                {
                    throw new InvalidDataException("cannot serialize(): " + type);
                }
                if (setType)
                {
                    writer.WriteByte(0x63);
                }
                WriteCustomType(writer, (RawCustomValue)value);
                return;
            }
            if (setType)
            {
                writer.WriteByte((byte)gpType);
            }
            GpType type4 = gpType;
            if (type4 != GpType.Null)
            {
                switch (type4)
                {
                    case GpType.Byte:
                        writer.WriteByte((byte)value);
                        return;

                    case GpType.Custom:
                        WriteCustomType(writer, (RawCustomValue)value);
                        return;

                    case GpType.Double:
                        writer.WriteDouble((double)value);
                        return;

                    case GpType.EventData:
                        WriteEventData(writer, (EventData)value);
                        return;

                    case GpType.Float:
                        writer.WriteSingle((float)value);
                        return;

                    case GpType.Hashtable:
                        WriteHashTable(writer, (Hashtable)value);
                        return;

                    case GpType.Integer:
                        writer.WriteInt32((int)value);
                        return;

                    case GpType.Short:
                        writer.WriteInt16((short)value);
                        return;

                    case GpType.Long:
                        writer.WriteInt64((long)value);
                        return;

                    case GpType.Boolean:
                        writer.WriteBoolean((bool)value);
                        return;

                    case GpType.OperationResponse:
                        WriteOperationResponse(writer, (OperationResponse)value);
                        return;

                    case GpType.OperationRequest:
                        WriteOperationRequest(writer, (OperationRequest)value);
                        return;

                    case GpType.String:
                        writer.WriteUTF((string)value);
                        return;

                    case GpType.Vector:
                        WriteVector(writer, (IList)value);
                        return;

                    case GpType.ByteArray:
                        WriteByteArray(writer, (byte[])value);
                        return;

                    case GpType.Array:
                        WriteArray(writer, (IList)value);
                        return;

                    case GpType.ObjectArray:
                        WriteObjectArray(writer, (IList)value);
                        return;

                    case GpType.Dictionary:
                        WriteDictionary(writer, value);
                        return;
                }
            }
            else
            {
                if (!setType)
                {
                    throw new InvalidOperationException("cannot serialize null values inside an array");
                }
                return;
            }
            throw new InvalidDataException("Unexpected - cannot serialize gp type: " + gpType);
        }

        /// <summary>
        /// Writes an array.
        /// </summary>
        /// <param name="writer"> The writer.</param>
        /// <param name="serObject">The ser object.</param>
        private static void WriteArray(IBinaryWriter writer, IList serObject)
        {
            Type elementType = serObject.GetType().GetElementType();
            GpType gpType = GpBinaryByteTypeConverter.GetGpType(elementType);
            if (gpType == GpType.Unknown)
            {
                if (elementType == typeof(RawCustomArray))
                {
                    writer.WriteInt16((short)serObject.Count);
                    for (int i = 0; i < serObject.Count; i++)
                    {
                        WriteCustomTypeArray(writer, (RawCustomArray)serObject[i], i == 0);
                    }
                }
                else
                {
                    CustomTypeInfo info;
                    if (!CustomTypeCache.TryGet(elementType, out info))
                    {
                        throw new InvalidDataException(string.Format("Arrays of type '{0}' are not supported.", elementType));
                    }
                    WriteCustomTypeArray(writer, info, serObject);
                }
            }
            else
            {
                writer.WriteInt16((short)serObject.Count);
                writer.WriteByte((byte)gpType);
                if (gpType == GpType.Dictionary)
                {
                    bool flag;
                    bool flag2;
                    WriteDictionaryHeader(writer, serObject, out flag, out flag2);
                    foreach (object obj2 in serObject)
                    {
                        WriteDictionaryElements(writer, obj2, flag, flag2);
                    }
                }
                else
                {
                    foreach (object obj3 in serObject)
                    {
                        Write(writer, obj3, false);
                    }
                }
            }
        }

        private static bool WriteArrayHeader(IBinaryWriter writer, Type type)
        {
            while (type.IsArray)
            {
                writer.WriteByte(0x79);
                type = type.GetElementType();
            }
            GpType gpType = GpBinaryByteTypeConverter.GetGpType(type);
            if (gpType == GpType.Unknown)
            {
                return false;
            }
            writer.WriteByte((byte)gpType);
            return true;
        }

        private static void WriteByteArray(IBinaryWriter writer, byte[] serObject)
        {
            writer.WriteInt32(serObject.Length);
            writer.WriteBytes(serObject);
        }

        private static void WriteCustomType(IBinaryWriter writer, RawCustomValue customType)
        {
            writer.WriteByte(customType.Code);
            writer.WriteInt16((short)customType.Data.Length);
            writer.WriteBytes(customType.Data);
        }

        private static void WriteCustomType(IBinaryWriter writer, CustomTypeInfo customTypeInfo, object value)
        {
            writer.WriteByte(customTypeInfo.Code);
            byte[] buffer = customTypeInfo.SerializeFunction(value);
            writer.WriteInt16((short)buffer.Length);
            writer.WriteBytes(buffer);
        }

        private static void WriteCustomTypeArray(IBinaryWriter writer, CustomTypeInfo customTypeInfo, IList list)
        {
            writer.WriteInt16((short)list.Count);
            writer.WriteByte(0x63);
            writer.WriteByte(customTypeInfo.Code);
            foreach (object obj2 in list)
            {
                byte[] buffer = customTypeInfo.SerializeFunction(obj2);
                writer.WriteInt16((short)buffer.Length);
                writer.WriteBytes(buffer);
            }
        }

        private static void WriteCustomTypeArray(IBinaryWriter writer, RawCustomArray array, [Optional, DefaultParameterValue(true)] bool writeArrayIdentifier)
        {
            if (writeArrayIdentifier)
            {
                writer.WriteByte(0x79);
            }
            writer.WriteInt16((short)array.Length);
            writer.WriteByte(0x63);
            writer.WriteByte(array.Code);
            for (int i = 0; i < array.Length; i++)
            {
                writer.WriteInt16((short)array[i].Length);
                writer.WriteBytes(array[i]);
            }
        }

        private static void WriteDictionary(IBinaryWriter writer, object dict)
        {
            Type[] genericArguments = dict.GetType().GetGenericArguments();
            bool setType = genericArguments[0] == typeof(object);
            bool flag2 = genericArguments[1] == typeof(object);
            if (setType)
            {
                writer.WriteByte(0);
            }
            else
            {
                GpType gpType = GpBinaryByteTypeConverter.GetGpType(genericArguments[0]);
                switch (gpType)
                {
                    case GpType.Unknown:
                    case GpType.Dictionary:
                        throw new InvalidDataException("Unexpected - cannot serialize Dictionary with key type: " + genericArguments[0]);
                }
                writer.WriteByte((byte)gpType);
            }
            if (flag2)
            {
                writer.WriteByte(0);
            }
            else
            {
                GpType type2 = GpBinaryByteTypeConverter.GetGpType(genericArguments[1]);
                if (type2 == GpType.Unknown)
                {
                    CustomTypeInfo info;
                    if (!CustomTypeCache.TryGet(genericArguments[1], out info))
                    {
                        throw new InvalidDataException("Unexpected - cannot serialize Dictionary with value type: " + genericArguments[1]);
                    }
                    writer.WriteByte(0x63);
                    writer.WriteByte(info.Code);
                }
                else
                {
                    writer.WriteByte((byte)type2);
                    if (type2 == GpType.Dictionary)
                    {
                        WriteDictionaryHeader(writer, genericArguments[1]);
                    }
                    else if ((type2 == GpType.Array) && !WriteArrayHeader(writer, genericArguments[1].GetElementType()))
                    {
                        throw new InvalidDataException("Unexpected - cannot serialize Dictionary with value type: " + genericArguments[1]);
                    }
                }
            }
            IDictionary dictionary = (IDictionary)dict;
            writer.WriteInt16((short)dictionary.Count);
            foreach (DictionaryEntry entry in dictionary)
            {
                Write(writer, entry.Key, setType);
                Write(writer, entry.Value, flag2);
            }
        }

        private static void WriteDictionaryElements(IBinaryWriter writer, object dict, bool setKeyType, bool setValueType)
        {
            IDictionary dictionary = (IDictionary)dict;
            writer.WriteInt16((short)dictionary.Count);
            foreach (DictionaryEntry entry in dictionary)
            {
                Write(writer, entry.Key, setKeyType);
                Write(writer, entry.Value, setValueType);
            }
        }

        private static void WriteDictionaryHeader(IBinaryWriter writer, Type dictType)
        {
            Type[] genericArguments = dictType.GetGenericArguments();
            if (genericArguments[0] == typeof(object))
            {
                writer.WriteByte(0);
            }
            else
            {
                GpType gpType = GpBinaryByteTypeConverter.GetGpType(genericArguments[0]);
                switch (gpType)
                {
                    case GpType.Unknown:
                    case GpType.Dictionary:
                        throw new InvalidDataException("Unexpected - cannot serialize Dictionary with key type: " + genericArguments[0]);
                }
                writer.WriteByte((byte)gpType);
            }
            if (genericArguments[1] == typeof(object))
            {
                writer.WriteByte(0);
            }
            else
            {
                GpType type2 = GpBinaryByteTypeConverter.GetGpType(genericArguments[1]);
                if (type2 == GpType.Unknown)
                {
                    throw new InvalidDataException("Unexpected - cannot serialize Dictionary with value type: " + genericArguments[1]);
                }
                writer.WriteByte((byte)type2);
                if (type2 == GpType.Dictionary)
                {
                    WriteDictionaryHeader(writer, genericArguments[1]);
                }
                else if ((type2 == GpType.Array) && !WriteArrayHeader(writer, genericArguments[1].GetElementType()))
                {
                    throw new InvalidDataException("Unexpected - cannot serialize Dictionary with value type: " + genericArguments[1]);
                }
            }
        }

        private static void WriteDictionaryHeader(IBinaryWriter writer, object dict, out bool setKeyType, out bool setValueType)
        {
            Type[] genericArguments = dict.GetType().GetGenericArguments();
            setKeyType = genericArguments[0] == typeof(object);
            setValueType = genericArguments[1] == typeof(object);
            if (setKeyType)
            {
                writer.WriteByte(0);
            }
            else
            {
                GpType gpType = GpBinaryByteTypeConverter.GetGpType(genericArguments[0]);
                switch (gpType)
                {
                    case GpType.Unknown:
                    case GpType.Dictionary:
                        throw new InvalidDataException("Unexpected - cannot serialize Dictionary with key type: " + genericArguments[0]);
                }
                writer.WriteByte((byte)gpType);
            }
            if (setValueType)
            {
                writer.WriteByte(0);
            }
            else
            {
                GpType type2 = GpBinaryByteTypeConverter.GetGpType(genericArguments[1]);
                if (type2 == GpType.Unknown)
                {
                    throw new InvalidDataException("Unexpected - cannot serialize Dictionary with value type: " + genericArguments[1]);
                }
                writer.WriteByte((byte)type2);
                if (type2 == GpType.Dictionary)
                {
                    WriteDictionaryHeader(writer, genericArguments[1]);
                }
            }
        }

        /// <summary>
        /// Writes an event data object.
        /// </summary>
        /// <param name="binaryWriter">The binary writer.</param>
        /// <param name="eventData">The event data.</param>
        public static void WriteEventData(IBinaryWriter binaryWriter, IEventData eventData)
        {
            binaryWriter.WriteByte(eventData.Code);
            if (eventData.Parameters == null)
            {
                binaryWriter.WriteInt16(0);
            }
            else
            {
                binaryWriter.WriteInt16((short)eventData.Parameters.Count);
                foreach (KeyValuePair<byte, object> pair in eventData.Parameters)
                {
                    binaryWriter.WriteByte(pair.Key);
                    Write(binaryWriter, pair.Value);
                }
            }
        }

        /// <summary>
        /// write hash table.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="serObject"> The ser object.</param>
        private static void WriteHashTable(IBinaryWriter writer, Hashtable serObject)
        {
            writer.WriteInt16((short)serObject.Count);
            foreach (DictionaryEntry entry in serObject)
            {
                Write(writer, entry.Key, true);
                Write(writer, entry.Value, true);
            }
        }

        private static void WriteObjectArray(IBinaryWriter writer, IList array)
        {
            writer.WriteInt16((short)array.Count);
            foreach (object obj2 in array)
            {
                Write(writer, obj2, true);
            }
        }

        /// <summary>
        /// Writes an operation request.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="operationRequest">The operation request.</param>
        public static void WriteOperationRequest(IBinaryWriter writer, OperationRequest operationRequest)
        {
            writer.WriteByte(operationRequest.OperationCode);
            if (operationRequest.Parameters != null)
            {
                writer.WriteInt16((short)operationRequest.Parameters.Count);
                foreach (KeyValuePair<byte, object> pair in operationRequest.Parameters)
                {
                    writer.WriteByte(pair.Key);
                    Write(writer, pair.Value);
                }
            }
            else
            {
                writer.WriteInt16(0);
            }
        }

        /// <summary>
        /// Writes an <see cref="T:Photon.SocketServer.OperationResponse"/>.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="operationResponse">The operation response.</param>
        public static void WriteOperationResponse(IBinaryWriter writer, OperationResponse operationResponse)
        {
            writer.WriteByte(operationResponse.OperationCode);
            writer.WriteInt16(operationResponse.ReturnCode);
            if (string.IsNullOrEmpty(operationResponse.DebugMessage))
            {
                writer.WriteByte(0x2a);
            }
            else
            {
                writer.WriteByte(0x73);
                writer.WriteUTF(operationResponse.DebugMessage);
            }
            if (operationResponse.Parameters == null)
            {
                writer.WriteInt16(0);
            }
            else
            {
                writer.WriteInt16((short)operationResponse.Parameters.Count);
                foreach (KeyValuePair<byte, object> pair in operationResponse.Parameters)
                {
                    writer.WriteByte(pair.Key);
                    Write(writer, pair.Value);
                }
            }
        }

        /// <summary>
        /// Writes a string.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="value">The value.</param>
        /// <param name="setType">If true the type is written.</param>
        internal static void WriteString(IBinaryWriter writer, string value, bool setType)
        {
            if (setType)
            {
                writer.WriteByte(0x73);
            }
            writer.WriteUTF(value);
        }

        /// <summary>
        ///  Writes a collection.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="serObject">The collection object.</param>
        /// <exception cref="T:System.ArrayTypeMismatchException">
        ///The vector supports just one type.
        ///</exception>
        private static void WriteVector(IBinaryWriter writer, ICollection serObject)
        {
            writer.WriteInt16((short)serObject.Count);
            if (serObject.Count > 0)
            {
                IEnumerator enumerator = serObject.GetEnumerator();
                enumerator.MoveNext();
                Type type = enumerator.Current.GetType();
                Write(writer, enumerator.Current, true);
                while (enumerator.MoveNext())
                {
                    if (enumerator.Current.GetType() != type)
                    {
                        throw new ArrayTypeMismatchException("Serialize: " + enumerator.Current.GetType().Name + " is illegal within an Array of " + type.Name + "s");
                    }
                    Write(writer, enumerator.Current, false);
                }
            }
        }
    }
}
