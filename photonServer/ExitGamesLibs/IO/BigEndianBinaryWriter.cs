using System;
using System.IO;
using System.Text;

namespace ExitGames.IO
{
    /// <summary>
    /// Provides methods to write binary data into a <see cref="T:System.IO.Stream">stream</see>.
    /// </summary>
    public sealed class BigEndianBinaryWriter : IBinaryWriter
    {
        /// <summary>
        /// The stream.
        /// </summary>
        private readonly Stream stream;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ExitGames.IO.BigEndianBinaryWriter"/> class.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        public BigEndianBinaryWriter(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            this.stream = stream;
        }

        /// <summary>
        /// Writes a byte to the stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="value">The value.</param>
        public static void WriteByte(Stream stream, byte value)
        {
            stream.WriteByte(value);
        }

        /// <summary>
        /// Writes a short to the stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="value">The value.</param>
        public static void WriteInt16(Stream stream, short value)
        {
            byte[] buffer = new byte[] { (byte)((value >> 8) & 0xff), (byte)(value & 0xff) };
            stream.Write(buffer, 0, 2);
        }

        /// <summary>
        /// Writes a bool to the stream.
        /// </summary>
        /// <param name="value">The value.</param>
        public void WriteBoolean(bool value)
        {
            this.WriteByte(value ? (byte)1 : (byte)0);
        }

        /// <summary>
        ///  Writes a <see cref="T:System.Byte"/> to the stream.
        /// </summary>
        /// <param name="value">The value.</param>
        public void WriteByte(byte value)
        {
            this.stream.WriteByte(value);
        }

        /// <summary>
        /// Write bytes to the stream.
        /// </summary>
        /// <param name="value">The bytes.</param>
        public void WriteBytes(byte[] value)
        {
            if (value != null)
            {
                this.stream.Write(value, 0, value.Length);
            }
        }

        /// <summary>
        /// Write a char to the stream.
        /// </summary>
        /// <param name="value"> The value.</param>
        public void WriteChar(char value)
        {
            this.WriteByte((BitConverter.GetBytes(value))[0]);
        }

        /// <summary>
        /// Writes an eight-byte floating-point value to the current stream 
        ///and advances the stream position by eight bytes.
        /// </summary>
        /// <param name="value">The eight-byte floating-point value to write.</param>
        public void WriteDouble(double value)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            for (int i = buffer.Length - 1; i >= 0; i--)
            {
                this.stream.WriteByte(buffer[i]);
            }
        }

        /// <summary>
        /// Writes a short (16 bit) to the stream.
        /// </summary>
        /// <param name="value">The value.</param>
        public void WriteInt16(short value)
        {
            byte[] buffer = new byte[] { 
                (byte)((value >> 8) & 0xff), 
                (byte)(value & 0xff) };
            this.stream.Write(buffer, 0, 2);
        }

        /// <summary>
        ///  Writes a int (32bit) to the stream.
        /// </summary>
        /// <param name="value">The value.</param>
        public void WriteInt32(int value)
        {
            byte[] buffer = new byte[] { 
                (byte)((value >> 0x18) & 0xff), 
                (byte)((value >> 0x10) & 0xff), 
                (byte)((value >> 8) & 0xff), 
                (byte)(value & 0xff) };
            this.stream.Write(buffer, 0, 4);
        }

        /// <summary>
        /// Writes a long (64 bit) to the stream.
        /// </summary>
        /// <param name="value"> The value.</param>
        public void WriteInt64(long value)
        {
            byte[] buffer = new byte[] { 
                (byte)((value >> 0x38) & 0xffL), 
                (byte)((value >> 0x30) & 0xffL), 
                (byte)((value >> 0x28) & 0xffL), 
                (byte)((value >> 0x20) & 0xffL), 
                (byte)((value >> 0x18) & 0xffL), 
                (byte)((value >> 0x10) & 0xffL), 
                (byte)((value >> 8) & 0xffL), 
                (byte)(value & 0xffL) };
            this.stream.Write(buffer, 0, 8);
        }

        /// <summary>
        /// Writes an four-byte floating-point value to the current stream 
        /// and advances the stream position by eight bytes.
        /// </summary>
        /// <param name="value">The eight-byte floating-point value to write.</param>
        public void WriteSingle(float value)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            for (int i = buffer.Length - 1; i >= 0; i--)
            {
                this.stream.WriteByte(buffer[i]);
            }
        }

        /// <summary>
        ///  Writes a UTF8 encoded string to the stream.
        /// </summary>
        /// <param name="value">  The value.</param>
        public void WriteUTF(string value)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(value);
            if (buffer.Length <= 0x7fff)
            {
                this.WriteInt16((short)buffer.Length);
            }
            else
            {
                this.WriteInt16(-1);
                this.WriteInt32(buffer.Length);
            }
            this.stream.Write(buffer, 0, buffer.Length);
        }

        /// <summary>
        ///  Gets the underlying stream.
        /// </summary>
        public Stream Stream
        {
            get
            {
                return this.stream;
            }
        }
    }
}
