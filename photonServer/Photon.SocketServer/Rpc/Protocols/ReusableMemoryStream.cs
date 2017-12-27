using System;
using System.IO;

namespace Photon.SocketServer.Rpc.Protocols
{
    /// <summary>
    /// The reusable memory stream.
    /// </summary>
    internal sealed class ReusableMemoryStream : Stream
    {
        /// <summary>
        /// The stream.
        /// </summary>
        [ThreadStatic]
        private static MemoryStream stream;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Photon.SocketServer.Rpc.Protocols.ReusableMemoryStream"/> class.
        /// </summary>
        public ReusableMemoryStream()
        {
            if (stream == null)
            {
                stream = new MemoryStream(0x100000);
            }
            else
            {
                stream.Capacity = 0x100000;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Photon.SocketServer.Rpc.Protocols.ReusableMemoryStream"/> class.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        public ReusableMemoryStream(byte[] buffer)
            : this()
        {
            stream.Write(buffer, 0, buffer.Length);
            stream.Position = 0L;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Photon.SocketServer.Rpc.Protocols.ReusableMemoryStream"/> class.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="count">The count.</param>
        public ReusableMemoryStream(byte[] buffer, int offset, int count)
            : this()
        {
            stream.Write(buffer, offset, count);
            stream.Position = 0L;
        }

        /// <summary>
        /// The dispose.
        /// </summary>
        /// <param name="disposing">The disposing.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                stream.SetLength(0L);
            }
        }

        /// <summary>
        /// The flush.
        /// </summary>
        public override void Flush()
        {
            stream.Flush();
        }

        /// <summary>
        /// The read.
        /// </summary>
        /// <param name="buffer"> The buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="count">The count.</param>
        /// <returns> The read bytes.</returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            return stream.Read(buffer, offset, count);
        }

        /// <summary>
        ///  The seek.
        /// </summary>
        /// <param name="offset">The offset.</param>
        /// <param name="origin">The origin.</param>
        /// <returns>The new position within the stream, calculated by combining the initial reference point and the offset</returns>
        public override long Seek(long offset, SeekOrigin origin)
        {
            return stream.Seek(offset, origin);
        }

        /// <summary>
        /// The set length.
        /// </summary>
        /// <param name="value">The value.</param>
        public override void SetLength(long value)
        {
            stream.SetLength(value);
        }

        /// <summary>
        /// The to array.
        /// </summary>
        /// <returns>the byte array</returns>
        public byte[] ToArray()
        {
            return stream.ToArray();
        }

        /// <summary>
        /// The write.
        /// </summary>
        /// <param name="buffer"> The buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="count">The count.</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            stream.Write(buffer, offset, count);
        }

        /// <summary>
        ///  Gets a value indicating whether CanRead.
        /// </summary>
        public override bool CanRead
        {
            get
            {
                return stream.CanRead;
            }
        }

        /// <summary>
        /// Gets a value indicating whether CanSeek.
        /// </summary>
        public override bool CanSeek
        {
            get
            {
                return stream.CanSeek;
            }
        }

        /// <summary>
        /// Gets a value indicating whether CanWrite.
        /// </summary>
        public override bool CanWrite
        {
            get
            {
                return stream.CanWrite;
            }
        }

        /// <summary>
        /// Gets Length.
        /// </summary>
        public override long Length
        {
            get
            {
                return stream.Length;
            }
        }

        /// <summary>
        /// Gets or sets Position.
        /// </summary>
        public override long Position
        {
            get
            {
                return stream.Position;
            }
            set
            {
                stream.Position = value;
            }
        }
    }
}
