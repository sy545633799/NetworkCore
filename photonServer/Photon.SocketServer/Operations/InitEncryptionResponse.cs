using Photon.SocketServer.Rpc;

namespace Photon.SocketServer.Operations
{
    internal class InitEncryptionResponse
    {
        // Properties
        [DataMember(Code = 1, IsOptional = true)]
        public byte[] ServerKey { get; set; }
    }
}
