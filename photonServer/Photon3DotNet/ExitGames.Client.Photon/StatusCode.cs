namespace ExitGames.Client.Photon
{
    using System;

    ///<summary>
    /// Enumeration of situations that change the peers internal status. 
    /// Used in calls to OnStatusChanged to inform your application of various situations that might happen.
    ///</summary>
    ///<remarks>
    /// Most of these codes are referenced somewhere else in the documentation when they are relevant to methods.
    ///</remarks>
    public enum StatusCode
    {
        /// <summary>
        /// the PhotonPeer is connected.<br/>See {@link PhotonListener#OnStatusChanged}*
        /// </summary>
        Connect = 0x400,

        /// <summary>
        /// the PhotonPeer just disconnected.<br/>See {@link PhotonListener#OnStatusChanged}*
        /// </summary>
        Disconnect = 0x401,

        /// <summary>
        /// the PhotonPeer encountered an exception and will disconnect, too.<br/>See {@link PhotonListener#OnStatusChanged}*
        /// </summary>
        Exception = 0x402,

        /// <summary>
        /// PhotonPeer outgoing queue is filling up. send more often.
        /// </summary>
        QueueOutgoingReliableWarning = 0x403,

        /// <summary>
        /// PhotonPeer outgoing queue is filling up. send more often.
        /// </summary>
        QueueOutgoingUnreliableWarning = 0x405,

        /// <summary>
        /// Sending command failed. Either not connected, or the requested channel is bigger than the number of initialized channels.
        /// </summary>
        SendError = 0x406,

        /// <summary>
        /// the PhotonPeer encountered an exception while opening the incoming connection to the server. The server could be down / not running or the client has no network or a misconfigured DNS.<br/>See {@link PhotonListener#OnStatusChanged}*
        /// </summary>
        ExceptionOnConnect = 0x3ff,

        /// <summary>
        /// Used on platforms that throw a security exception on connect. Unity3d does this, e.g., if a webplayer build could not fetch a policy-file from a remote server.
        /// </summary>
        SecurityExceptionOnConnect = 0x3fe,

        /// <summary>
        /// PhotonPeer outgoing queue is filling up. send more often.
        /// </summary>
        QueueOutgoingAcksWarning = 0x407,

        /// <summary>
        /// PhotonPeer incoming queue is filling up. Dispatch more often.
        /// </summary>
        QueueIncomingReliableWarning = 0x409,

        /// <summary>
        /// PhotonPeer incoming queue is filling up. Dispatch more often.
        /// </summary>
        QueueIncomingUnreliableWarning = 0x40b,

        /// <summary>
        /// PhotonPeer incoming queue is filling up. Dispatch more often.
        /// </summary>
        QueueSentWarning = 0x40d,

        /// <summary>
        /// Exception, if a server cannot be connected. Most likely, the server is not responding. Ask user to try again later.
        /// </summary>
        InternalReceiveException = 0x40f,

        /// <summary>
        /// Disconnection due to a timeout (client did no longer receive ACKs from server).
        /// </summary>
        TimeoutDisconnect = 0x410,

        /// <summary>
        /// Disconnect by server due to timeout (received a disconnect command, cause server misses ACKs of client).
        /// </summary>
        DisconnectByServer = 0x411,

        /// <summary>
        /// Disconnect by server due to concurrent user limit reached (received a disconnect command).
        /// </summary>
        DisconnectByServerUserLimit = 0x412,

        /// <summary>
        /// Disconnect by server due to server's logic (received a disconnect command).
        /// </summary>
        DisconnectByServerLogic = 0x413,

        /// <summary>
        /// Tcp Router Response. Only used when Photon is setup as TCP router! Routing is ok.
        /// </summary>
        [Obsolete("TCP routing was removed after becoming obsolete.")]
        TcpRouterResponseOk = 0x414,

        /// <summary>
        /// Tcp Router Response. Only used when Photon is setup as TCP router! Routing node unknown. Check client connect values.
        /// </summary>
        [Obsolete("TCP routing was removed after becoming obsolete.")]
        TcpRouterResponseNodeIdUnknown = 0x415,

        /// <summary>
        /// Tcp Router Response. Only used when Photon is setup as TCP router! Routing endpoint unknown.
        /// </summary>
        [Obsolete("TCP routing was removed after becoming obsolete.")]
        TcpRouterResponseEndpointUnknown = 0x416,

        /// <summary>
        /// Tcp Router Response. Only used when Photon is setup as TCP router! Routing not setup yet. Connect again.
        /// </summary>
        [Obsolete("TCP routing was removed after becoming obsolete.")]
        TcpRouterResponseNodeNotReady = 0x417,

        /// <summary>
        /// (1048) Value for OnStatusChanged()-call, when the encryption-setup for secure communication finished successfully.
        /// </summary>
        EncryptionEstablished = 0x418,

        /// <summary>
        /// (1049) Value for OnStatusChanged()-call, when the encryption-setup failed for some reason. Check debug logs.
        /// </summary>
        EncryptionFailedToEstablish = 0x419
    }
}
