using System;
using System.IO;

namespace ExitGames.IO
{
    /// <summary>
    /// DataInputStreamWrapper provides functions to Read binary data from a stream.
    /// </summary>
    public class BigEndianBinaryReader : IBinaryReader
    {
        /// <summary>
        /// The stream to read from.
        /// </summary>
        private readonly Stream stream;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ExitGames.IO.BigEndianBinaryReader"/> class.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="stream"/> is null.
        ///</exception>
        public BigEndianBinaryReader(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            this.stream = stream;
        }

        /// <summary>
        /// Reads a sequence of bytes from the current stream and advances 
        /// the position within the stream by the number of bytes read.
        /// </summary>
        /// <param name="buffer">An array of bytes. When this method returns, the buffer contains 
        /// the specified byte array with the values between offset and (offset + count - 1) 
        /// replaced by the bytes read from the current source. </param>
        /// <param name="offset">The zero-based byte offset in buffer at which to begin storing 
        /// the data read from the current stream.</param>
        /// <param name="count">The maximum number of bytes to be read from the current stream. </param>
        /// <returns>
        /// The total number of bytes read into the buffer. 
        /// This can be less than the number of bytes requested if that many 
        /// bytes are not currently available, or zero (0) if the end of the 
        /// stream has been reached. 
        /// </returns>
        public int Read(byte[] buffer, int offset, int count)
        {
            return this.stream.Read(buffer, offset, count);
        }

        /// <summary>
        /// Reads a Boolean value from the current stream and advances the current 
        /// position of the stream by one byte.
        /// </summary>
        /// <returns>Type: <see cref="T:System.Boolean"/>
        /// True if the byte is nonzero; otherwise, false. </returns>
        public bool ReadBoolean()
        {
            return Convert.ToBoolean(this.ReadByte());
        }

        /// <summary>
        /// Reads the next byte from the current stream and advances the current 
        /// position of the stream by one byte.
        /// </summary>
        /// <returns>The read byte.</returns>
        public byte ReadByte()
        {
            return ReadBytesFromStream(this.stream, 1)[0];
        }

        /// <summary>
        /// Reads bytes.
        /// </summary>
        /// <param name="length">The length.</param>
        /// <returns> A byte array.</returns>
        public byte[] ReadBytes(int length)
        {
            return ReadBytesFromStream(this.stream, length);
        }

        /// <summary>
        /// Read bytes from a stream.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="length">The length.</param>
        /// <returns>A byte array.</returns>
        private static byte[] ReadBytesFromStream(Stream data, int length)
        {
            byte[] buffer = new byte[length];
            data.Read(buffer, 0, length);
            return buffer;
        }

        /// <summary>
        ///  Reads a char.
        /// </summary>
        /// <returns> The read char.</returns>
        public char ReadChar()
        {
            return Convert.ToChar(this.ReadByte());
        }

        /// <summary>
        /// Reads an 8-byte floating point value from the current stream and advances 
        /// the current position of the stream by eight bytes.
        /// </summary>
        /// <returns>The read double.</returns>
        public double ReadDouble()
        {
            byte[] buffer = ReadBytesFromStream(this.stream, 8);
            Array.Reverse(buffer);
            return BitConverter.ToDouble(buffer, 0);
        }

        /// <summary>
        /// Reads a 2-byte signed integer from the current stream and advances the 
        /// current position of the stream by two bytes.
        /// </summary>
        /// <returns>Type: <see cref="T:System.Int16"/>
        /// A 2-byte signed integer read from the current stream.</returns>
        /// <exception cref="T:System.ObjectDisposedException">
        ///The stream is closed.
        /// </exception>
        public short ReadInt16()
        {
            byte[] buffer = ReadBytesFromStream(this.stream, 2);
            return (short)((buffer[0] << 8) | buffer[1]);
        }

        /// <summary>
        /// Reads a 4-byte signed integer from the current stream 
        /// and advances the current position of the stream by four bytes.
        /// </summary>
        /// Type:
        /// <see cref="T:System.Int32"/>
        /// A 4-byte signed integer read from the current stream.
        /// <returns>The read int 32.</returns>
        public int ReadInt32()
        {
            byte[] buffer = ReadBytesFromStream(this.stream, 4);
            return ((((buffer[0] << 0x18) | (buffer[1] << 0x10)) | (buffer[2] << 8)) | buffer[3]);
        }

        /// <summary>
        /// Read a long (64 bit) from the stream.
        /// </summary>
        /// <returns>A long.</returns>
        public long ReadInt64()
        {
            byte[] buffer = ReadBytesFromStream(this.stream, 8);
            return (long)((((((((buffer[0] << 0x38) | (buffer[1] << 0x30)) | (buffer[2] << 40)) | (buffer[3] << 0x20)) | (buffer[4] << 0x18)) | (buffer[5] << 0x10)) | (buffer[6] << 8)) | buffer[7]);
        }

        /// <summary>
        /// Reads an 4-byte floating point value from the current stream and advances the current position of the stream by four bytes.
        /// </summary>
        /// <returns>The read single.</returns>
        public float ReadSingle()
        {
            byte[] buffer = ReadBytesFromStream(this.stream, 4);
            Array.Reverse(buffer);
            return BitConverter.ToSingle(buffer, 0);
        }

        /// <summary>
        /// Gets the underlying stream of the <see cref="T:ExitGames.IO.BigEndianBinaryReader"/>.
        /// </summary>
        public Stream BaseStream
        {
            get
            {
                return this.stream;
            }
        }
    }
}
