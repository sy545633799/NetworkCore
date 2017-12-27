using System;

namespace Photon.SocketServer.Rpc
{
    /// <summary>
    /// When applied to the member of a type, specifies that the member is part of a data contract and should by serialized. 
    /// </summary>
    public class DataMemberAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the code. 
        /// </summary>
        public byte Code { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether IsOptional. 
        /// </summary>
        public bool IsOptional { get; set; }

        /// <summary>
        /// Gets or sets the data members name. 
        /// </summary>
        public string Name { get; set; }
    }
}
