using System;

namespace Photon.SocketServer.Rpc.Reflection
{
    /// <summary>
    /// This attribute is used to mark methods to be reflected by the <see cref="T:Photon.SocketServer.Rpc.Reflection.OperationDispatcher"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class OperationAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets OperationCode.
        /// </summary>
        public byte OperationCode { get; set; }
    }

}
