namespace ExitGames.Client.Photon.Lite
{
    using System;

    ///<summary>
    /// Lite - Operation Codes.
    /// This enumeration contains the codes that are given to the Lite Application's
    /// operations. Instead of sending "Join", this enables us to send the byte 255.
    ///</summary>
    ///<remarks>
    /// Other applications (the MMO demo or your own) could define other operations and other codes.
    /// If your game is built as extension of Lite, don't re-use these codes for your custom events.
    ///</remarks>
    public static class LiteOpCode
    {
        /// <summary>
        /// (255) Code for OpJoin, to get into a room.
        /// </summary>
        public const byte Join = 0xff;

        /// <summary>
        /// (254) Code for OpLeave, to get out of a room.
        /// </summary>
        public const byte Leave = 0xfe;

        /// <summary>
        /// (253) Code for OpRaiseEvent (not same as eventCode).
        /// </summary>
        public const byte RaiseEvent = 0xfd;

        /// <summary>
        /// (252) Code for OpSetProperties.
        /// </summary>
        public const byte SetProperties = 0xfc;

        /// <summary>
        /// (251) Operation code for OpGetProperties.
        /// </summary>
        public const byte GetProperties = 0xfb;

        /// <summary>
        /// (248) Operation code to change interest groups in Rooms (Lite application and extending ones).
        /// </summary>
        public const byte ChangeGroups = 0xf8;

        [Obsolete("Exchanging encrpytion keys is done internally in the lib now. Don't expect this operation-result.")]
        public const byte ExchangeKeysForEncryption = 250;
    }
}
