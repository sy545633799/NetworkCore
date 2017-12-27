namespace Photon.SocketServer.PeerConnectionStateMachine
{
    internal interface IConnectionState
    {
        // Methods
        bool TransitDisconnect(PeerBase peer);
        bool TransitDisposeConnected(PeerBase peer);
        bool TransitDisposeDisconnected(PeerBase peer);
        bool TransitOnDisconnect(PeerBase peer);

        // Properties
        ConnectionState Value { get; }
    }
}
