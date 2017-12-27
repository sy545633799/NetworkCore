namespace Photon.SocketServer.ServerToServer
{
    /// <summary>
    /// The byte buffer. 
    /// </summary>
    internal class ByteBuffer
    {
        /// <summary>
        /// The binary data. 
        /// </summary>
        private readonly byte[] binaryData;

        /// <summary>
        ///  Initializes a new instance of the <see cref="T:Photon.SocketServer.ServerToServer.ByteBuffer"/> class.
        /// </summary>
        /// <param name="size"> The size.</param>
        public ByteBuffer(int size)
        {
            this.BytesRead = 0;
            this.binaryData = new byte[size];
        }

        /// <summary>
        ///  The read.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="count">The count.</param>
        /// <returns>    The number of bytes read</returns>
        public int Read(byte[] buffer, int offset, int count)
        {
            int num = this.binaryData.Length - this.BytesRead;
            int num2 = count - offset;
            if (num2 >= num)
            {
                System.Buffer.BlockCopy(buffer, offset, this.binaryData, this.BytesRead, num);
                this.BytesRead += num;
                return num;
            }
            System.Buffer.BlockCopy(buffer, offset, this.binaryData, this.BytesRead, num2);
            this.BytesRead += num2;
            return num2;
        }

        /// <summary>
        /// Gets Buffer.
        /// </summary>
        public byte[] Buffer
        {
            get
            {
                return this.binaryData;
            }
        }

        /// <summary>
        /// Gets BytesRead.
        /// </summary>
        public int BytesRead { get; private set; }

        /// <summary>
        /// Gets a value indicating whether Complete.
        /// </summary>
        public bool Complete
        {
            get
            {
                return (this.BytesRead == this.Size);
            }
        }

        /// <summary>
        ///  Gets Size.
        /// </summary>
        public int Size
        {
            get
            {
                return this.binaryData.Length;
            }
        }
    }
}
