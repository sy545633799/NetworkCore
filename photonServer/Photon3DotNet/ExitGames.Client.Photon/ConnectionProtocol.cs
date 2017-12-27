namespace ExitGames.Client.Photon
{
    /// <summary>
    /// These are the options that can be used as underlying transport protocol. 
    /// </summary>
    public enum ConnectionProtocol : byte
    {
        /// <summary>
        /// Use UDP to connect to Photon, which allows you to send operations reliable or unreliable on demand.
        /// </summary>
        Udp = 0,

        /// <summary>
        /// Use TCP to connect to Photon.
        /// </summary>
        Tcp = 1,

        /// <summary>
        /// Use HTTP connections to connect a Photon Master (not available in regular Photon SDK).
        /// </summary>
        Http = 2,

        RHttp = 3
    }
}
