namespace ExitGames.Client.Photon
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    ///<summary>
    /// Value range for a Peer's connection and initialization state, as returned by the PeerState property.
    ///</summary>
    ///<remarks>
    /// While this is not the same as the StatusCode of IPhotonPeerListener.OnStatusChanged(), it directly relates to it.
    /// In most cases, it makes more sense to build a game's state on top of the OnStatusChanged() as you get changes.
    ///</remarks>
    public enum PeerStateValue : byte
    {
        /// <summary>
        /// The peer is disconnected and can't call Operations. Call Connect().
        /// </summary>
        Disconnected = 0,

        /// <summary>
        /// The peer is establishing the connection: opening a socket, exchanging packages with Photon.
        /// </summary>
        Connecting = 1,

        /// <summary>
        /// The peer is connected and initialized (selected an application). You can now use operations.
        /// </summary>
        Connected = 3,

        /// <summary>
        /// The peer is disconnecting. It sent a disconnect to the server, which will acknowledge closing the connection.
        /// </summary>
        Disconnecting = 4,

        ///<summary>The connection is established and now sends the application name to Photon.</summary>
        ///<remarks>You set the "application name" by calling PhotonPeer.Connect().</remarks>
        InitializingApplication = 10
    }
}
