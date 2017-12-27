using System;

namespace Photon.SocketServer.Rpc.ValueTypes
{
    /// <summary>
    /// Instances of this class will be created for unknown custom types sent by a client if 
    /// the <see cref="P:Photon.SocketServer.Protocol.AllowRawCustomValues"/> property is set to true.
    /// </summary>
    public sealed class RawCustomValue : IEquatable<RawCustomValue>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Photon.SocketServer.Rpc.ValueTypes.RawCustomValue"/> class.
        /// </summary>
        /// <param name="code">The code of the custom type.</param>
        /// <param name="data">The serialized data.</param>
        public RawCustomValue(byte code, byte[] data)
        {
            this.Code = code;
            this.Data = data;
        }

        public bool Equals(RawCustomValue other)
        {
            if (other.Code != this.Code)
            {
                return false;
            }
            if (other.Data.Length != this.Data.Length)
            {
                return false;
            }
            for (int i = 0; i < this.Data.Length; i++)
            {
                if (other.Data[i] != this.Data[i])
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Gets or sets the type code of the custom type.
        /// </summary>
        public byte Code { get; set; }

        /// <summary>
        ///  Gets or sets the serialized data of the custom type.
        /// </summary>
        public byte[] Data { get; set; }
    }
}
