namespace Photon.SocketServer.PeerConnectionStateMachine
{
    /// <summary>
    /// Possible states of a <see cref="T:Photon.SocketServer.PeerBase"/>.
    /// </summary>
    internal enum ConnectionState
    {
        Connected,
        Disconnected,
        Disposed
    }
}
