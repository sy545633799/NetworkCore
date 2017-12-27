
namespace Photon.SocketServer.PeerConnectionStateMachine
{
    internal sealed class Connected : IConnectionState
    {
        // Fields
        public static readonly Connected Instance = new Connected();

        // Methods
        private Connected()
        {
        }

        public bool TransitDisconnect(PeerBase peer)
        {
            return (peer.TransitConnectionState(Disconnecting.Instance, this) || peer.ConnectionStateImpl.TransitDisconnect(peer));
        }

        public bool TransitDisposeConnected(PeerBase peer)
        {
            return (peer.TransitConnectionState(DisposeDisconnecting.Instance, this) || peer.ConnectionStateImpl.TransitDisposeConnected(peer));
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
                return ConnectionState.Connected;
            }
        }
    }
}
