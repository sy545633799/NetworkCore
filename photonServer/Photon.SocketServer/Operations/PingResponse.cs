using Photon.SocketServer.Rpc;

namespace Photon.SocketServer.Operations
{
    internal class PingResponse : Operation
    {
        // Methods
        public PingResponse(IRpcProtocol protocol, OperationRequest operationRequest)
            : base(protocol, operationRequest)
        {
        }

        public PingResponse(int clientTimeStamp, int serverTimeStamp)
        {
            this.ClientTimeStamp = clientTimeStamp;
            this.ServerTimeStamp = serverTimeStamp;
        }

        // Properties
        [DataMember(Code = 1, IsOptional = false)]
        public int ClientTimeStamp { get; set; }

        [DataMember(Code = 2, IsOptional = false)]
        public int ServerTimeStamp { get; set; }
    }
}
