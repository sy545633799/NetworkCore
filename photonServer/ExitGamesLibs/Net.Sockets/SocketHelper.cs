using System.Net.Sockets;

namespace ExitGames.Net.Sockets
{
    /// <summary>
    /// Internal helper methods.
    /// </summary>
    internal static class SocketHelper
    {
        /// <summary>
        /// Formats the socket exception.
        /// </summary>
        /// <param name="ex">The ex.</param>
        /// <returns>The format socket exception.</returns>
        internal static string FormatSocketException(SocketException ex)
        {
            return string.Format("ErrorCode={0}, SocketErrorCode={1}, NativeErrorCode={2}", ex.ErrorCode, ex.SocketErrorCode, ex.NativeErrorCode);
        }
    }
}
