using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using ExitGames.IO;
using ExitGames.Logging;

namespace Photon.SocketServer.Rpc.Protocols.Amf3
{
    /// <summary>
    /// Provides methods to read Action Message Format (AMF 3) binary data. 
    /// </summary>
    /// <remarks>Object references are references to an already inlined object. Object references start at 0 and are in the order that the objects are defined. Object references include dates, arrays, and objects. 
    /// </remarks>
    internal sealed class Amf3Reader
    {
        /// <summary>
        /// amf base date. 
        /// </summary>
        private static readonly DateTime amfBaseDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// The binary reader. 
        /// </summary>
        private readonly IBinaryReader binaryReader;

        /// <summary>
        /// class def references. 
        /// </summary>
        private readonly List<Amf3ClassDefinition> classDefReferences = new List<Amf3ClassDefinition>();

        /// <summary>
        /// The log. 
        /// </summary>
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// object references. 
        /// </summary>
        private readonly List<object> objectReferences = new List<object>();

        /// <summary>
        /// string references. 
        /// </summary>
        private readonly List<string> stringReferences = new List<string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Photon.SocketServer.Rpc.Protocols.Amf3.Amf3Reader"/> class.
        /// </summary>
        /// <param name="binaryReader">
        /// <see cref="T:ExitGames.IO.IBinaryReader"/> used to write data to the underling stream.
        /// </param>
        public Amf3Reader(IBinaryReader binaryReader)
        {
            this.binaryReader = binaryReader;
        }

        /// <summary>
        /// Reads the next object from the underling stream.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <returns>The object that has been read.</returns>
        public bool Read(out object result)
        {
            Amf3TypeMarker amfTypeMarker = (Amf3TypeMarker)this.binaryReader.ReadByte();
            return this.Read(amfTypeMarker, out result);
        }

        /// <summary>
        /// Reads an object from the underling stream.
        /// </summary>
        /// <param name="amfTypeMarker">
        /// An <see cref="T:Photon.SocketServer.Rpc.Protocols.Amf3.Amf3TypeMarker"/> which indicates which type of object to read.
        /// </param>
        /// <param name="result">
        /// The result.
        /// </param>
        /// <returns>
        /// The object that has been read.
        /// </returns>
        private bool Read(Amf3TypeMarker amfTypeMarker, out object result)
        {
            switch (amfTypeMarker)
            {
                case Amf3TypeMarker.Undefined:
                case Amf3TypeMarker.Null:
                    result = null;
                    return true;

                case Amf3TypeMarker.False:
                    result = false;
                    return true;

                case Amf3TypeMarker.True:
                    result = true;
                    return true;

                case Amf3TypeMarker.Integer:
                    result = this.ReadInteger();
                    return true;

                case Amf3TypeMarker.Double:
                    result = this.binaryReader.ReadDouble();
                    return true;

                case Amf3TypeMarker.String:
                    string str;
                    if (!this.ReadString(out str))
                    {
                        result = null;
                        return true;
                    }
                    result = str;
                    return true;

                case Amf3TypeMarker.Date:
                    return this.ReadDateTime(out result);

                case Amf3TypeMarker.Array:
                    return this.ReadArray(out result);

                case Amf3TypeMarker.Object:
                    return this.ReadObject(out result);

                case Amf3TypeMarker.ByteArray:
                    result = this.ReadByteArray();
                    return true;
            }
            if (log.IsDebugEnabled)
            {
                log.DebugFormat("Unknown amf3 type marker {0} at position {1}", new object[] { amfTypeMarker, this.binaryReader.BaseStream.Position });
            }
            result = null;
            return false;
        }

        /// <summary>
        /// Reads an array from the underling stream.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <returns>The read array.</returns>
        /// <remarks>
        /// For associative arrays, data is in name/value pairs, similar to how objects are encoded (type 2). 
        /// If the integer-data was zero, then the name/value pairs are terminated by a single-null 
        /// string (that is simply a 1). If the integer-data was 1, then the name/value pairs are terminated 
        ///by a single-null value string as well, but the null-value string will also have a final value. 
        ///This second case occurs when the associative array has defined a key that is either 0 or \’\’ (empty string). 
        /// If both 0 and \’\’ are set in the array then the first name/value pair sent will have a null 
        ///  string for the key and the final name/value pair sent will also have a null string for a key. 
        ///  (I’m not sure if there is any other way to get a null string key.) The zero value seems to be always 
        /// the one sent first in case of the two null strings.
        /// </remarks>
        private bool ReadArray(out object result)
        {
            int flags = this.ReadInteger();
            if ((flags & 1) == 1)
            {
                return this.ReadInlineArray(flags, out result);
            }
            int num2 = flags >> 1;
            if (num2 < this.objectReferences.Count)
            {
                result = this.objectReferences[num2];
                return true;
            }
            if (log.IsDebugEnabled)
            {
                log.DebugFormat("Array object reference index out of range at position {0}", new object[] { this.binaryReader.BaseStream.Position });
            }
            result = null;
            return false;
        }

        /// <summary>
        /// Reads a byte array from a binary reader.
        /// </summary>
        /// <returns>A byte array containing data read from the underlying stream.</returns>   
        /// <remarks>
        /// ActionScript 3.0 introduces a new type to hold an Array of bytes, namely ByteArray. 
        /// AMF 3 serializes this type using a variable length encoding 29-bit integer for the 
        ///byte-length prefix followed by the raw bytes of the ByteArray.
        ///ByteArray instances can be sent as a reference to a previously occurring ByteArray 
        /// instance by using an index to the implicit object reference table.
        /// </remarks>
        private byte[] ReadByteArray()
        {
            int num = this.ReadInteger();
            if ((num & 1) == 0)
            {
                int num2 = num >> 1;
                if (num2 < this.objectReferences.Count)
                {
                    byte[] buffer = this.objectReferences[num2] as byte[];
                    if (buffer != null)
                    {
                        return buffer;
                    }
                    if (log.IsDebugEnabled)
                    {
                        log.DebugFormat("Byte array object reference has index to wrong type at {0} position {1}", new object[] { this.objectReferences[num2].GetType(), this.binaryReader.BaseStream.Position });
                    }
                }
                else if (log.IsDebugEnabled)
                {
                    log.DebugFormat("Byte array object reference index out of range at position {0}", new object[] { this.binaryReader.BaseStream.Position });
                }
                return null;
            }
            int length = num >> 1;
            byte[] item = this.binaryReader.ReadBytes(length);
            this.objectReferences.Add(item);
            return item;
        }

        /// <summary>
        /// read date time.
        /// </summary>
        /// <param name="result">The result <see cref="T:System.DateTime"/> .</param>
        /// <returns>true if read was successfull. </returns>
        private bool ReadDateTime(out object result)
        {
            int num = this.ReadInteger();
            if ((num & 1) == 1)
            {
                double num2 = this.binaryReader.ReadDouble();
                result = amfBaseDate.AddMilliseconds(num2);
                this.objectReferences.Add(result);
                return true;
            }
            int num3 = num >> 1;
            if (num3 < this.objectReferences.Count)
            {
                result = this.objectReferences[num3];
                return true;
            }
            if (log.IsDebugEnabled)
            {
                log.DebugFormat("DateTime object reference index out of range at position {0}", new object[] { this.binaryReader.BaseStream.Position });
            }
            result = null;
            return false;
        }

        /// <summary>
        /// Reads dynamic object properies.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <returns>True on success.</returns>
        private bool ReadDynamicObjectProperties(out object result)
        {
            string str;
            Hashtable hashtable = new Hashtable();
            if (this.ReadString(out str))
            {
                while (str != string.Empty)
                {
                    object obj2;
                    if (this.Read(out obj2))
                    {
                        hashtable.Add(str, obj2);
                        if (!this.ReadString(out str))
                        {
                            result = null;
                            return false;
                        }
                    }
                    else
                    {
                        result = null;
                        return false;
                    }
                }
                result = hashtable;
                return true;
            }
            result = null;
            return false;
        }

        /// <summary>
        /// Reads an inline array from the underling stream
        /// </summary>
        /// <param name="flags">the flags</param>
        /// <param name="result">The result.</param>
        /// <returns>A array object read from the current stream. </returns>
        private bool ReadInlineArray(int flags, out object result)
        {
            string str;
            int num = flags >> 1;
            if (!this.ReadString(out str))
            {
                result = null;
                return false;
            }
            if (str != string.Empty)
            {
                Hashtable hashtable = new Hashtable();
                this.objectReferences.Add(hashtable);
                while (str != string.Empty)
                {
                    object obj2;
                    if (this.Read(out obj2))
                    {
                        hashtable.Add(str, obj2);
                        if (!this.ReadString(out str))
                        {
                            result = null;
                            return false;
                        }
                    }
                    else
                    {
                        result = null;
                        return false;
                    }
                }
                for (int j = 0; j < num; j++)
                {
                    object obj3;
                    if (this.Read(out obj3))
                    {
                        hashtable.Add(j, obj3);
                    }
                    else
                    {
                        result = null;
                        return false;
                    }
                }
                result = hashtable;
                return true;
            }
            object[] item = new object[num];
            this.objectReferences.Add(item);
            for (int i = 0; i < num; i++)
            {
                object obj4;
                if (this.Read(out obj4))
                {
                    item[i] = obj4;
                }
                else
                {
                    result = null;
                    return false;
                }
            }
            result = item;
            return true;
        }

        /// <summary>
        /// read inline dynamic object.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <returns> The read inline dynamic object.</returns>
        /// <remarks>
        /// If there is a class-def reference there are no property names 
        /// and the number of values is equal to the number of properties 
        /// in the class-def.
        /// </remarks>
        private bool ReadInlineDynamicObject(out object result)
        {
            string str;
            string str2;
            if (!this.ReadString(out str))
            {
                result = null;
                return false;
            }
            Amf3ClassDefinition item = new Amf3ClassDefinition
            {
                ClassName = str,
                IsDynamic = true
            };
            this.classDefReferences.Add(item);
            List<string> list = new List<string>();
            Hashtable hashtable = new Hashtable();
            if (this.ReadString(out str2))
            {
                while (str2 != string.Empty)
                {
                    object obj2;
                    list.Add(str2);
                    if (this.Read(out obj2))
                    {
                        hashtable.Add(str2, obj2);
                        if (!this.ReadString(out str2))
                        {
                            result = null;
                            return false;
                        }
                    }
                    else
                    {
                        result = null;
                        return false;
                    }
                }
                item.PropertyNames = list.ToArray();
                result = hashtable;
                return true;
            }
            result = null;
            return false;
        }

        /// <summary>
        /// read inline non dynamic object.
        /// </summary>
        /// <param name="flags">The flags.</param>
        /// <param name="result">The result.</param>
        /// <returns>the non dynamic object</returns>
        private bool ReadInlineNonDynamicObject(int flags, out object result)
        {
            string str;
            int num = flags >> 4;
            if (!this.ReadString(out str))
            {
                result = null;
                return false;
            }
            string[] strArray = new string[num];
            for (int i = 0; i < num; i++)
            {
                string str2;
                if (!this.ReadString(out str2))
                {
                    result = null;
                    return false;
                }
                strArray[i] = str2;
            }
            Hashtable hashtable = new Hashtable();
            for (int j = 0; j < num; j++)
            {
                object obj2;
                if (this.Read(out obj2))
                {
                    hashtable.Add(strArray[j], obj2);
                }
                else
                {
                    result = null;
                    return false;
                }
            }
            Amf3ClassDefinition item = new Amf3ClassDefinition
            {
                ClassName = str,
                PropertyNames = strArray
            };
            this.classDefReferences.Add(item);
            result = hashtable;
            return true;
        }

        /// <summary>
        ///  Reads the next 1-4 bytes (variable bit length) from the binary stream 
        /// and converts the result to an unsigned integer.
        /// </summary>
        /// <returns> The read integer.</returns>
        /// <remarks>
        /// AMF 3 represents smaller integers with fewer bytes using the most
        /// significant bit of each byte. The worst case uses 32-bits to represent 
        /// a 29-bit number, which is what we would have done with no compression.
        /// To save space it is an integer that can be 1-4 bytes long. The first bit 
        ///of the first three bytes determine if the next byte is included (1) in 
        ///this integer-data or not (0). The last byte, if present, is read completely (8 bits). 
        ///The first bits are then removed from the first three bytes and the remaining bits concatenated 
        ///to form a big-endian integer. 
        ///The integer is negative if it is the full 29 bits long and the first bit is set (1). 
        ///This uses Two's complement notation and is therefore identical to normal signed integer behaviour.
        ///- 0x00000000 - 0x0000007F : 0xxxxxxx
        /// - 0x00000080 - 0x00003FFF : 1xxxxxxx 0xxxxxxx
        /// - 0x00004000 - 0x001FFFFF : 1xxxxxxx 1xxxxxxx 0xxxxxxx
        ///- 0x00200000 - 0x3FFFFFFF : 1xxxxxxx 1xxxxxxx 1xxxxxxx xxxxxxxx
        ///- 0x40000000 - 0xFFFFFFFF : throw range exception
        /// </remarks>
        private int ReadInteger()
        {
            int num = 1;
            byte num2 = this.binaryReader.ReadByte();
            uint num3 = 0;
            while (((num2 & 0x80) != 0) && (num < 4))
            {
                num3 = num3 << 7;
                num3 |= (uint)(num2 & 0x7f);
                num2 = this.binaryReader.ReadByte();
                num++;
            }
            if (num < 4)
            {
                num3 = num3 << 7;
                num3 |= num2;
            }
            else
            {
                num3 = num3 << 8;
                num3 |= num2;
                if ((num3 & 0x10000000) != 0)
                {
                    num3 |= 0xe0000000;
                }
            }
            return (int)num3;
        }

        /// <summary>
        /// Reads an object from the underling stream.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <returns>The read object.</returns>
        private bool ReadObject(out object result)
        {
            int flags = this.ReadInteger();
            if ((flags & 1) == 0)
            {
                int num2 = flags >> 1;
                if (num2 < this.objectReferences.Count)
                {
                    result = this.objectReferences[num2];
                    return true;
                }
                if (log.IsDebugEnabled)
                {
                    log.DebugFormat("Object reference index out of range at position {0}", new object[] { this.binaryReader.BaseStream.Position });
                }
                result = null;
                return false;
            }
            if ((flags & 2) == 0)
            {
                int num3 = flags >> 2;
                if (num3 < this.classDefReferences.Count)
                {
                    Amf3ClassDefinition classDefinition = this.classDefReferences[num3];
                    if (classDefinition.IsDynamic)
                    {
                        return this.ReadDynamicObjectProperties(out result);
                    }
                    return this.ReadObjectProperties(classDefinition, out result);
                }
                if (log.IsDebugEnabled)
                {
                    log.DebugFormat("Class definition index out of range at position {0}", new object[] { this.binaryReader.BaseStream.Position });
                }
                result = null;
                return false;
            }
            switch (((flags >> 2) & 3))
            {
                case 0:
                    if (this.ReadInlineNonDynamicObject(flags, out result))
                    {
                        break;
                    }
                    return false;

                case 1:
                    if (log.IsDebugEnabled)
                    {
                        log.DebugFormat("Externalizable object is not supported, at position {0}", new object[] { this.binaryReader.BaseStream.Position });
                    }
                    result = null;
                    return false;

                case 2:
                    if (this.ReadInlineDynamicObject(out result))
                    {
                        break;
                    }
                    return false;

                default:
                    if (log.IsDebugEnabled)
                    {
                        log.DebugFormat("Invalid amf3 object format, at position {0}", new object[] { this.binaryReader.BaseStream.Position });
                    }
                    result = null;
                    return false;
            }
            this.objectReferences.Add(result);
            return true;
        }

        /// <summary>
        /// read object properties.
        /// </summary>
        /// <param name="classDefinition">The class definition.</param>
        /// <param name="result">The result.</param>
        /// <returns>the object properties</returns>
        private bool ReadObjectProperties(Amf3ClassDefinition classDefinition, out object result)
        {
            Hashtable hashtable = new Hashtable(classDefinition.PropertyNames.Length);
            foreach (string str in classDefinition.PropertyNames)
            {
                object obj2;
                if (this.Read(out obj2))
                {
                    hashtable.Add(str, obj2);
                }
                else
                {
                    result = null;
                    return false;
                }
            }
            result = hashtable;
            return true;
        }

        /// <summary>
        ///  Read and deserialize a string
        /// Strings can be sent as a reference to a previously
        /// occurring String by using an index to the implicit string reference table.
        /// Strings are encoding using UTF-8 - however the header may either
        /// describe a string literal or a string reference.
        /// - string = 0x06 string-data
        /// - string-data = integer-data [ modified-utf-8 ]
        ///  - modified-utf-8 = *OCTET
        /// </summary>
        /// <param name="result"> The result.</param>
        /// <returns> true on success.</returns>
        private bool ReadString(out string result)
        {
            int num = this.ReadInteger();
            if ((num & 1) == 0)
            {
                int num2 = num >> 1;
                if (num2 < this.stringReferences.Count)
                {
                    result = this.stringReferences[num2];
                    return true;
                }
                if (log.IsDebugEnabled)
                {
                    log.DebugFormat("String reference index out of range at position {0}", new object[] { this.binaryReader.BaseStream.Position });
                }
                result = null;
                return false;
            }
            int length = num >> 1;
            if (length > 0)
            {
                byte[] bytes = this.binaryReader.ReadBytes(length);
                result = Encoding.UTF8.GetString(bytes);
                this.stringReferences.Add(result);
                return true;
            }
            result = string.Empty;
            return true;
        }
    }
}
