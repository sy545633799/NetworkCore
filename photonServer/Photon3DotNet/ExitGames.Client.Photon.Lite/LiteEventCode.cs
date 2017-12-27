namespace ExitGames.Client.Photon.Lite
{
    ///<summary>
    /// Lite - Event codes.
    /// These codes are defined by the Lite application's logic on the server side.
    /// Other application's won't necessarily use these.
    ///</summary>
    ///<remarks>If your game is built as extension of Lite, don't re-use these codes for your custom events.</remarks>
    public static class LiteEventCode
    {
        /// <summary>
        /// (255) Event Join: someone joined the game
        /// </summary>
        public const byte Join = 0xff;

        /// <summary>
        /// (254) Event Leave: someone left the game
        /// </summary>
        public const byte Leave = 0xfe;

        /// <summary>
        /// (253) Event PropertiesChanged
        /// </summary>
        public const byte PropertiesChanged = 0xfd;
    }
}
