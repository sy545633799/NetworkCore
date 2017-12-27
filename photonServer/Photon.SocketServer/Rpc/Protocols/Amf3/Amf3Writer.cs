using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ExitGames.IO;

namespace Photon.SocketServer.Rpc.Protocols.Amf3
{
    /// <summary>
    /// Provides methods to write Action Message Format (AMF 3) binary data. 
    /// </summary>
    internal sealed class Amf3Writer
    {
        /// <summary>
        /// Value used for Datetime conversion. 
        /// </summary>
        private static readonly DateTime amfBaseDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// Used to write native types to the underling stream. 
        /// </summary>
        private readonly IBinaryWriter binaryWriter;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Photon.SocketServer.Rpc.Protocols.Amf3.Amf3Writer"/> class
        /// based on the supplied <see cref="T:ExitGames.IO.IBinaryWriter"/>. 
        /// </summary>
        /// <param name="binaryWriter">The binary writer</param>
        public Amf3Writer(IBinaryWriter binaryWriter)
        {
            if (binaryWriter == null)
            {
                throw new ArgumentNullException("binaryWriter");
            }
            this.binaryWriter = binaryWriter;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Photon.SocketServer.Rpc.Protocols.Amf3.Amf3Writer"/> class 
        /// based on the supplied stream.
        /// </summary>
        /// <param name="stream">The ouput stream.</param>
        public Amf3Writer(Stream stream)
        {
            this.binaryWriter = new BigEndianBinaryWriter(stream);
        }

        /// <summary>
        /// Serializes an object.
        /// </summary>
        /// <param name="value"> The value.</param>
        /// <exception cref="T:System.IO.InvalidDataException">
        ///  The type of the <paramref name="value"/> can not be serialized.
        /// </exception>
        public void Write(object value)
        {
            if (value == null)
            {
                this.WriteNull();
            }
            else
            {
                switch (Type.GetTypeCode(value.GetType()))
                {
                    case TypeCode.Boolean:
                        this.WriteBoolean((bool)value);
                        return;

                    case TypeCode.SByte:
                        this.WriteInteger((long)((sbyte)value));
                        return;

                    case TypeCode.Byte:
                        this.WriteInteger((long)((byte)value));
                        return;

                    case TypeCode.Int16:
                        this.WriteInteger((long)((short)value));
                        return;

                    case TypeCode.UInt16:
                        this.WriteInteger((long)((ushort)value));
                        return;

                    case TypeCode.Int32:
                        this.WriteInteger((long)((int)value));
                        return;

                    case TypeCode.UInt32:
                        this.WriteInteger((long)((uint)value));
                        return;

                    case TypeCode.Int64:
                        this.WriteInteger((long)value);
                        return;

                    case TypeCode.UInt64:
                        this.WriteInteger((long)((ulong)value));
                        return;

                    case TypeCode.Single:
                        this.WriteNumber((double)((float)value));
                        return;

                    case TypeCode.Double:
                        this.WriteNumber((double)value);
                        return;

                    case TypeCode.DateTime:
                        this.WriteDate((DateTime)value);
                        return;

                    case TypeCode.String:
                        this.WriteString((string)value, true);
                        return;
                }
                byte[] buffer = value as byte[];
                if (buffer != null)
                {
                    this.WriteByteArray(buffer);
                }
                else
                {
                    IList list = value as IList;
                    if (list != null)
                    {
                        this.WriteArray(list);
                    }
                    else if (value is ICollection<KeyValuePair<string, object>>)
                    {
                        this.WriteNameValueCollection(value as ICollection<KeyValuePair<string, object>>);
                    }
                    else
                    {
                        IDictionary dictionary = value as IDictionary;
                        if (dictionary == null)
                        {
                            throw new InvalidDataException(string.Format("Type {0} is not supported.", value.GetType()));
                        }
                        this.WriteDictionary(dictionary);
                    }
                }
            }
        }

        /// <summary>
        /// write array.
        /// </summary>
        /// <param name="value">The value.</param>
        public void WriteArray(IList value)
        {
            this.WriteArrayHeader(true, value.Count);
            this.WriteString(string.Empty, false);
            foreach (object obj2 in value)
            {
                this.Write(obj2);
            }
        }

        /// <summary>
        ///  write array header.
        /// </summary>
        /// <param name="isInline">The is inline.</param>
        /// <param name="integerIndexedElementCount">The integer indexed element count.</param>
        private void WriteArrayHeader(bool isInline, int integerIndexedElementCount)
        {
            this.binaryWriter.WriteByte(9);
            int num = isInline ? 1 : 0;
            num |= integerIndexedElementCount << 1;
            this.WriteIntegerValue(num);
        }

        /// <summary>
        /// Writes a boolean value to the underling stream.
        /// </summary>
        /// <param name="value"></param>
        public void WriteBoolean(bool value)
        {
            if (!value)
            {
                this.binaryWriter.WriteByte(2);
            }
            else
            {
                this.binaryWriter.WriteByte(3);
            }
        }

        /// <summary>
        /// Writes a byte array to the underlying stream.
        /// </summary>
        /// <param name="value">A byte array containing the data to write.</param>
        public void WriteByteArray(byte[] value)
        {
            this.binaryWriter.WriteByte(12);
            int num = (value.Length << 1) | 1;
            this.WriteIntegerValue(num);
            this.binaryWriter.WriteBytes(value);
        }

        /// <summary>
        /// write date.
        /// </summary>
        /// <param name="value">The value.</param>
        public void WriteDate(DateTime value)
        {
            this.binaryWriter.WriteByte(8);
            this.binaryWriter.WriteByte(1);
            double totalMilliseconds = value.Subtract(amfBaseDate).TotalMilliseconds;
            this.binaryWriter.WriteDouble(totalMilliseconds);
        }

        /// <summary>
        ///  write name value collection.
        /// </summary>
        /// <param name="value">The value.</param>
        public void WriteDictionary(IDictionary value)
        {
            this.WriteArrayHeader(true, 0);
            foreach (object obj2 in value.Keys)
            {
                this.WriteString(obj2.ToString(), false);
                object obj3 = value[obj2];
                this.Write(obj3);
            }
            this.WriteString(string.Empty, false);
        }

        /// <summary>
        ///  write integer.
        /// </summary>
        /// <param name="value">The value.</param>
        public void WriteInteger(long value)
        {
            if ((value > 0xfffffffL) || (value < -268435456L))
            {
                this.WriteNumber((double)value);
            }
            else
            {
                this.binaryWriter.WriteByte(4);
                this.WriteIntegerValue((int)value);
            }
        }

        /// <summary>
        ///  write integer value.
        /// </summary>
        /// <param name="value">The value.</param>
        private void WriteIntegerValue(int value)
        {
            value &= 0x1fffffff;
            if (value < 0x80)
            {
                this.binaryWriter.WriteByte((byte)value);
            }
            else if (value < 0x4000)
            {
                this.binaryWriter.WriteByte((byte)(((value >> 7) & 0x7f) | 0x80));
                this.binaryWriter.WriteByte((byte)(value & 0x7f));
            }
            else if (value < 0x200000)
            {
                this.binaryWriter.WriteByte((byte)(((value >> 14) & 0x7f) | 0x80));
                this.binaryWriter.WriteByte((byte)(((value >> 7) & 0x7f) | 0x80));
                this.binaryWriter.WriteByte((byte)(value & 0x7f));
            }
            else
            {
                this.binaryWriter.WriteByte((byte)(((value >> 0x16) & 0x7f) | 0x80));
                this.binaryWriter.WriteByte((byte)(((value >> 15) & 0x7f) | 0x80));
                this.binaryWriter.WriteByte((byte)(((value >> 8) & 0x7f) | 0x80));
                this.binaryWriter.WriteByte((byte)(value & 0xff));
            }
        }

        /// <summary>
        /// write name value collection.
        /// </summary>
        /// <param name="value">The value.</param>
        public void WriteNameValueCollection(ICollection<KeyValuePair<string, object>> value)
        {
            this.WriteArrayHeader(true, 0);
            foreach (KeyValuePair<string, object> pair in value)
            {
                this.WriteString(pair.Key, false);
                this.Write(pair.Value);
            }
            this.WriteString(string.Empty, false);
        }

        /// <summary>
        /// write null.
        /// </summary>
        public void WriteNull()
        {
            this.binaryWriter.WriteByte(1);
        }

        /// <summary>
        /// write number.
        /// </summary>
        /// <param name="value">The value.</param>
        public void WriteNumber(double value)
        {
            this.binaryWriter.WriteByte(5);
            this.binaryWriter.WriteDouble(value);
        }

        /// <summary>
        /// Writes a string to the underling stream.
        /// </summary>
        /// <param name="value">the value</param>
        /// <param name="writeTypeMarker"> indicates wheter to write the type marker</param>
        public void WriteString(string value, bool writeTypeMarker)
        {
            if (writeTypeMarker)
            {
                this.binaryWriter.WriteByte(6);
            }
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            int num = (bytes.Length << 1) | 1;
            this.WriteIntegerValue(num);
            this.binaryWriter.WriteBytes(bytes);
        }
    }
}
