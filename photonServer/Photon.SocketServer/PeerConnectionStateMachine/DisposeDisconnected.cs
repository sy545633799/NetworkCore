using ExitGames.Logging;

namespace Photon.SocketServer.PeerConnectionStateMachine
{
    internal class DisposeDisconnected : IConnectionState
    {
        // Fields
        public static readonly DisposeDisconnected Instance = new DisposeDisconnected();
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        // Methods
        private DisposeDisconnected()
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
            return (peer.TransitConnectionState(Disposed.Instance, this) || peer.ConnectionStateImpl.TransitDisposeDisconnected(peer));
        }

        public bool TransitOnDisconnect(PeerBase peer)
        {
            log.Warn("Unexpected TransitOnDisconnect call");
            return false;
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
