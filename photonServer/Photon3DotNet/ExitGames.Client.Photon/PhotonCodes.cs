namespace ExitGames.Client.Photon
{
    internal static class PhotonCodes
    {
        /// <summary>
        ///  Param code. Used in internal op: InitEncryption.
        /// </summary>
        internal static byte ClientKey = 1;

        /// <summary>
        /// Code of internal op: InitEncryption.
        /// </summary>
        internal static byte InitEncryption = 0;

        /// <summary>
        /// Encryption-Mode code. Used in internal op: InitEncryption.
        /// </summary>
        internal static byte ModeKey = 2;

        /// <summary>
        /// Result code for any (internal) operation.
        /// </summary>
        public const byte Ok = 0;

        /// <summary>
        /// Param code. Used in internal op: InitEncryption.
        /// </summary>
        internal static byte ServerKey = 1;
    }
}
