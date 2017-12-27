namespace ExitGames.Client.Photon
{
    /// <summary>
    /// Level / amount of DebugReturn callbacks. Each debug level includes output for lower ones: OFF, ERROR, WARNING, INFO, ALL.
    /// </summary>
    public enum DebugLevel : byte
    {
        /// <summary>
        /// Most complete workflow description (but lots of debug output), info, warnings and errors.
        /// </summary>
        ALL = 5,

        /// <summary>
        /// Only error descriptions.
        /// </summary>
        ERROR = 1,

        /// <summary>
        /// Information about internal workflows, warnings and errors.
        /// </summary>
        INFO = 3,

        /// <summary>
        /// No debug out.
        /// </summary>
        OFF = 0,

        /// <summary>
        /// Warnings and errors.
        /// </summary>
        WARNING = 2
    }
}
