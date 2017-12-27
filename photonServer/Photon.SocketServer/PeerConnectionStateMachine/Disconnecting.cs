namespace Photon.SocketServer.PeerConnectionStateMachine
{
    internal sealed class Disconnecting : IConnectionState
    {
        // Fields
        public static readonly Disconnecting Instance = new Disconnecting();

        // Methods
        private Disconnecting()
        {
        }

        public bool TransitDisconnect(PeerBase peer)
        {
            return false;
        }

        public bool TransitDisposeConnected(PeerBase peer)
        {
            if (peer.TransitConnectionState(DisposeDisconnecting.Instance, this))
            {
                return false;
            }
            return peer.ConnectionStateImpl.TransitDisposeConnected(peer);
        }

        public bool TransitDisposeDisconnected(PeerBase peer)
        {
            return false;
        }

        public bool TransitOnDisconnect(PeerBase peer)
        {
            return (peer.TransitConnectionState(Disconnected.Instance, this) || peer.ConnectionStateImpl.TransitOnDisconnect(peer));
        }

        // Properties
        public ConnectionState Value
        {
            get
            {
                return ConnectionState.Disconnected;
            }
        }
    }
}
