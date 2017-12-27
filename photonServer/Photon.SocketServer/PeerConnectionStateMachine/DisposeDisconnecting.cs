namespace Photon.SocketServer.PeerConnectionStateMachine
{
    internal class DisposeDisconnecting : IConnectionState
    {
        // Fields
        public static readonly DisposeDisconnecting Instance = new DisposeDisconnecting();

        // Methods
        private DisposeDisconnecting()
        {
        }

        public bool TransitDisconnect(PeerBase peer)
        {
            return false;
        }

        public bool TransitDisposeConnected(PeerBase peer)
        {
            return false;
        }

        public bool TransitDisposeDisconnected(PeerBase peer)
        {
            return false;
        }

        public bool TransitOnDisconnect(PeerBase peer)
        {
            return (peer.TransitConnectionState(DisposeDisconnected.Instance, this) || peer.ConnectionStateImpl.TransitOnDisconnect(peer));
        }

        // Properties
        public ConnectionState Value
        {
            get
            {
                return ConnectionState.Disposed;
            }
        }
    }
}
