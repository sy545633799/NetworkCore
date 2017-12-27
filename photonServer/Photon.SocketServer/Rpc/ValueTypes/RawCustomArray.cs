using System;
using System.Collections;

namespace Photon.SocketServer.Rpc.ValueTypes
{
    /// <summary>
    /// Instances of this class will be created for arrays of unknown custom types sent by a client if 
    /// the <see cref="P:Photon.SocketServer.Protocol.AllowRawCustomValues"/> property is set to true.
    /// </summary>
    public sealed class RawCustomArray : IEnumerable, IEquatable<RawCustomArray>
    {
        /// <summary>
        /// Gets the type code of the custom type.
        /// </summary>
        public readonly byte Code;

        /// <summary>
        /// Gets the serialized data for each element of the custom type array.
        /// </summary>
        private readonly byte[][] data;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Photon.SocketServer.Rpc.ValueTypes.RawCustomArray"/> class.
        /// </summary>
        /// <param name="code">The code of the custom type.</param>
        /// <param name="count">The number of element in the array.</param>
        public RawCustomArray(byte code, int count)
        {
            this.Code = code;
            this.data = new byte[count][];
        }

        public bool Equals(RawCustomArray other)
        {
            if (other.Code != this.Code)
            {
                return false;
            }
            if (other.Length != this.Length)
            {
                return false;
            }
            for (int i = 0; i < this.Length; i++)
            {
                if (other[i].Length != this[i].Length)
                {
                    return false;
                }
                for (int j = 0; j < this[i].Length; j++)
                {
                    if (other[i][j] != this[i][j])
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Gets an enumerator for the custom arrays.
        /// </summary>
        /// <returns>The enumerator.</returns>
        public IEnumerator GetEnumerator()
        {
            return this.data.GetEnumerator();
        }

        /// <summary>
        /// Gets the byte array at the specified index.
        /// </summary>
        /// <param name="i"> The index.</param>
        /// <returns>The byte array at the specified index.</returns>
        public byte[] this[int i]
        {
            get
            {
                return this.data[i];
            }
            set
            {
                this.data[i] = value;
            }
        }

        /// <summary>
        ///  Gets the array's length.
        /// </summary>
        public int Length
        {
            get
            {
                return this.data.Length;
            }
        }
    }
}
