using ExitGames.Logging;

namespace Photon.SocketServer.PeerConnectionStateMachine
{
    internal sealed class Disconnected : IConnectionState
    {
        // Fields
        public static readonly Disconnected Instance = new Disconnected();
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        // Methods
        private Disconnected()
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
            return (peer.TransitConnectionState(Disposed.Instance, this) || peer.ConnectionStateImpl.TransitDisposeConnected(peer));
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
                return ConnectionState.Disconnected;
            }
        }
    }
}
