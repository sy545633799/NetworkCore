using Photon.SocketServer.Rpc;

namespace Photon.SocketServer.Operations
{
    internal class InitEncryptionRequest : Operation
    {
        // Methods
        public InitEncryptionRequest(IRpcProtocol protocol, OperationRequest operationRequest)
            : base(protocol, operationRequest)
        {
        }

        public InitEncryptionRequest(byte[] clientKey, byte mode)
        {
            this.ClientKey = clientKey;
            this.Mode = mode;
        }

        // Properties
        [DataMember(Code = 1, IsOptional = false)]
        public byte[] ClientKey { get; set; }

        [DataMember(Code = 2, IsOptional = true)]
        public byte Mode { get; set; }
    }

}
