using PhotonHostRuntimeInterfaces;

namespace Photon.SocketServer
{
    internal interface IManagedPeer
    {
        // Methods
        bool Application_OnDisconnect(DisconnectReason reasonCode, string reasonDetail, int rtt, int rttVariance, int numFailures);
        void Application_OnReceive(byte[] data, SendParameters sendParameters, int rtt, int rttVariance, int numFailure);
        void Application_OnSendBufferEmpty();
    }
}
