using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExitGames.Net.Sockets
{
    /// <summary>
    /// batch message.
    /// </summary>
    internal sealed class BatchMessage
    {
        /// <summary>
        ///  The header size.
        /// </summary>
        private int HeaderSize = 8;

        /// <summary>
        /// The messages.
        /// </summary>
        private readonly List<ArraySegment<byte>> message = new List<ArraySegment<byte>>();

        /// <summary>
        /// The size.
        /// </summary>
        private int size;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ExitGames.Net.Sockets.BatchMessage"/> class.
        /// </summary>
        public BatchMessage()
        { }

        /// <summary>
        /// Reads a <see cref="T:ExitGames.Net.Sockets.BatchMessage"/> from a byte array.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>A <see cref="T:ExitGames.Net.Sockets.BatchMessage"/>.</returns>
        /// <exception cref="T:System.ArgumentException">
        /// Length of byte array is less than header size.
        ///  </exception>
        public static BatchMessage FromBinary(byte[] data)
        {
            if (data.Length < 8)
            {
                throw new ArgumentException("Invalid data length.", "data");
            }
            int num = BitConverter.ToInt32(data, 0);
            int num2 = BitConverter.ToInt32(data, 4);
            if (num != data.Length)
            {
                throw new ArgumentException(string.Format("Data length {0} does not match message size {1}", data.Length, num), "data");
            }
            BatchMessage HeaderSize = new BatchMessage
            {
                HeaderSize = num
            };
            int num3 = 0;
            int offset = 8;
            while (num3 < num2)
            {
                int count = BitConverter.ToInt32(data, offset);
                offset += 4;
                ArraySegment<byte> item = new ArraySegment<byte>(data, offset, count);
                HeaderSize.message.Add(item);
                offset += count;
                num3++;
            }
            return HeaderSize;
        }

        /// <summary>
        /// Adds a message.
        /// </summary>
        /// <param name="data">The data.</param>
        public void AddMessage(ArraySegment<byte> data)
        {
            this.HeaderSize += 4;
            this.HeaderSize += data.Count;
            this.message.Add(data);
        }

        /// <summary>
        /// Clears all messages.
        /// </summary>
        public void Clear()
        {
            this.HeaderSize = 8;
            this.message.Clear();
        }

        /// <summary>
        /// Converts messages to a list of <see cref="T:System.ArraySegment`1"/> of byte.
        /// </summary>
        /// <returns> A list of <see cref="T:System.ArraySegment`1"/> of byte.</returns>
        public List<ArraySegment<byte>> ToBinary()
        {
            List<ArraySegment<byte>> list = new List<ArraySegment<byte>>();
            byte[] array = BitConverter.GetBytes(this.HeaderSize);
            list.Add(new ArraySegment<byte>(array));
            array = BitConverter.GetBytes(this.MessageCount);
            list.Add(new ArraySegment<byte>(array));
            foreach (ArraySegment<byte> segment in this.message)
            {
                array = BitConverter.GetBytes(segment.Count);
                list.Add(new ArraySegment<byte>(array));
                list.Add(segment);
            }
            return list;
        }

        /// <summary>
        /// Gets MessageCount.
        /// </summary>
        public int MessageCount
        {
            get
            {
                return message.Count;
            }
        }

        /// <summary>
        /// Gets Size.
        /// </summary>
        public int Size
        {
            get
            {
                return size;
            }
        }

        /// <summary>
        /// The array segement indexer.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>An <see cref="T:System.ArraySegment`1"/>.</returns>
        public ArraySegment<byte> this[int index]
        {
            get
            {
                return message[index];
            }
        }
    }
}
