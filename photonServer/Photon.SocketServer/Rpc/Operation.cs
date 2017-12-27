using Photon.SocketServer.Diagnostics;

namespace Photon.SocketServer.Rpc
{
    public class Operation : DataContract
    {
        // Fields
        private readonly OperationRequest request;
        private long? startTime;

        // Methods
        public Operation()
        {
        }

        public Operation(IRpcProtocol protocol, OperationRequest request)
            : base(protocol, request.Parameters)
        {
            this.request = request;
        }

        public void OnComplete()
        {
            if (this.startTime.HasValue)
            {
                PhotonCounter.OnOperationCompleted(this.startTime.GetValueOrDefault());
            }
        }

        public void OnStart()
        {
            this.startTime = new long?(PhotonCounter.GetTimestamp());
        }

        // Properties
        public OperationRequest OperationRequest
        {
            get
            {
                return this.request;
            }
        }
    }
}
