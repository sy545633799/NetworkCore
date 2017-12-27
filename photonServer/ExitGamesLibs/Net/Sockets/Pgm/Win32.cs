namespace ExitGames.Net.Sockets.Pgm.Win32
{
    /// <summary>
    /// Contains reliable multicast specific constants.
    /// </summary>
    /// <remarks>
    ///  Declared in Wsrm.h (Microsoft Windows SDK).
    ///  </remarks>
    class PgmConstants
    {
        /// <summary>
        /// PGM protocol for reliable multicast.
        /// </summary>
        const int IpProtoRm = 113;
    }

    /// <summary>
    /// WindowSizeInBytes = (RateKbitsPerSec / 8) * WindowSizeInMSecs
    /// The RM_SEND_WINDOW structure specifies the Reliable Multicast send window. 
    ///   This structure is used with the RM_RATE_WINDOW_SIZE socket option.
    /// </summary>
    /// <remarks>
    /// Any combination of the three available members may be set for a given socket option call. 
    ///   For example, one, any two, or all three members may be specified during a setsockopt 
    ///  function call. Regardless of settings, Windows enforces the following ratio: 
    ///  TransmissionRate == (WindowSizeBytes/WindowSizeMSecs) * 8. 
    /// As such, setting any two parameters effectively sets the third to ensure optimum performance. 
    /// The combination of these members can affect the resources used on a PGM sender's computer. 
    ///  For example, a large transmission rate value combined with a large window size results in 
    ///  more required buffer space.
    /// </remarks>
    internal struct _RM_SEND_WINDOW
    {
        /// <summary>
        /// Transmission rate for the send window, in kilobits per second.
        /// </summary>
        public uint RateKbitsPerSec;            // Send rate
        /// <summary>
        /// Window size for the send window, in milliseconds.
        /// </summary>
        public uint WindowSizeInMSecs;
        /// <summary>
        ///  Window size for the session, in bytes.
        /// </summary>
        public uint WindowSizeInBytes;
    }

    /// <summary>
    /// Socket options that apply to sockets created for the IPv4 address 
    ///  family (AF_INET) with the protocol parameter to the socket function 
    /// specified as reliable multicast (IpProtoRm). 
    /// </summary>
    /// <remarks>Declared in Wsrm.h (Microsoft Windows SDK).</remarks>
    internal enum PgmSocketOptions
    {
        /// <summary>
        /// r m_ optionsbase.
        /// </summary>
        RM_OPTIONSBASE = 0x3e8,
        /// <summary>
        /// Sender only. Sets the transmission rate limit, 
        ///window advance time, and window size.
        /// </summary>
        RM_RATE_WINDOW_SIZE = 1001,
        /// <summary>
        /// Sender only. Specifies size of the next message to be sent, 
        /// in bytes. Meaningful only to message mode sockets (SOCK_RDM). 
        /// Can be set while the session is in progress.
        /// </summary>
        RM_SET_MESSAGE_BOUNDARY,
        /// <summary>
        /// Not implemented - flush the entire data (window) right now.
        /// </summary>
        RM_FLUSHCACHE,
        /// <summary>
        /// Sender only. The optval parameter specifies the method used when 
        /// advancing the trailing edge send window. The optval parameter can 
        ///  only be E_WINDOW_ADVANCE_BY_TIME (the default). 
        ///   Note that E_WINDOW_USE_AS_DATA_CACHE is not supported.
        /// </summary>
        RM_SENDER_WINDOW_ADVANCE_METHOD,
        /// <summary>
        /// Sender only. Retrieves statistics for the sending session.
        /// </summary>
        RM_SENDER_STATISTICS,
        /// <summary>
        /// Sender only. Percentage of window size allowed to be requested by late-joining 
        /// receivers upon session acceptance. Maximum value is 75% (default is zero). 
        /// Disable this setting by calling again with value set to zero.
        /// </summary>
        RM_LATEJOIN,
        /// <summary>
        /// Sender only. Sets the sending interface IP address in network byte order. 
        /// </summary>
        RM_SET_SEND_IF,
        /// <summary>
        /// Receiver only. Adds an interface on which to listen (the default is the 
        ///  first local interface enumerated). The optval parameter specifies the 
        /// network interface in network byte order to add. The value specified 
        ///  replaces the default interface on the first call for a given socket, 
        ///  and adds other interfaces on subsequent calls. 
        ///  To obtain INADDR_ANY behavior, each network interface must be added separately.
        /// </summary>
        RM_ADD_RECEIVE_IF,
        /// <summary>
        /// Receiver only. Removes an interface added using RM_ADD_RECEIVE_IF. 
        /// The optval parameter specifies the network interface in network byte 
        ///  order to delete.
        /// </summary>
        RM_DEL_RECEIVE_IF,
        /// <summary>
        ///  Sender only. Specifies the incremental advance rate for the trailing 
        ///  edge send window (default is 15%). Maximum value is 50%.
        /// </summary>
        RM_SEND_WINDOW_ADV_RATE,
        /// <summary>
        /// Sender only. Notifies sender to apply forward error correction 
        /// techniques to send repair data. FEC has three modes: pro-active parity 
        /// packets only, OnDemand parity packets only, or both. 
        /// See RM_FEC_INFO structure for more information.
        /// </summary>
        RM_USE_FEC,
        /// <summary>
        /// Sender only. Sets the maximum time to live (TTL) setting for 
        ///multicast packets. Maximum and default value is 255.
        /// </summary>
        RM_SET_MCAST_TTL,
        /// <summary>
        /// Receiver only. Retrieves statistics for the receiving session.
        /// </summary>
        RM_RECEIVER_STATISTICS,
        /// <summary>
        ///Receiver only. Specifies whether a high bandwidth LAN (100Mbps+) 
        ///   connection is used.
        /// </summary>
        RM_HIGH_SPEED_INTRANET_OPT,
    }
}
