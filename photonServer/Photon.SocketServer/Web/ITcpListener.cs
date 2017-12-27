using PhotonHostRuntimeInterfaces;

namespace Photon.SocketServer.Web
{
    internal interface ITcpListener
    {
        // Methods
        void OnPingResponse(int serverTime, int clienttime);
        void OnReceive(byte[] data, byte channelId, MessageReliablity reliablity);
    }
}
