using Photon.SocketServer.Rpc;

namespace Photon.SocketServer.Operations
{
    internal class PingRequest : Operation
    {
        // Methods
        public PingRequest(int clientTimeStamp)
        {
            this.ClientTimeStamp = clientTimeStamp;
        }

        public PingRequest(IRpcProtocol protocol, OperationRequest operationRequest)
            : base(protocol, operationRequest)
        {
        }

        // Properties
        [DataMember(Code = 1, IsOptional = false)]
        public int ClientTimeStamp { get; set; }
    }
}
