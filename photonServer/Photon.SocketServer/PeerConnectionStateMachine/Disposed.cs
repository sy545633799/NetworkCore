using ExitGames.Logging;

namespace Photon.SocketServer.PeerConnectionStateMachine
{
    internal sealed class Disposed : IConnectionState
    {
        // Fields
        public static readonly Disposed Instance = new Disposed();
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        // Methods
        private Disposed()
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
