using System.Runtime.InteropServices;

namespace Photon.SocketServer.Rpc.Protocols.Amf3
{
    /// <summary>
    /// amf 3 class definition. 
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct Amf3ClassDefinition
    {
        /// <summary>
        /// Gets or sets ClassName. 
        /// </summary>
        public string ClassName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this class type is dynamic. 
        /// </summary>
        /// <value>true if this instance is dynamic; otherwise, <c>false</c>. </value>
        public bool IsDynamic { get; set; }

        /// <summary>
        /// Gets or sets PropertyNames. 
        /// </summary>
        public string[] PropertyNames { get; set; }
    }
}
