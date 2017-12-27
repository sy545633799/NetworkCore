using System;

namespace ExitGames.Extensions
{
    /// <summary>
    /// Date time extension methods.
    /// </summary>
    public static class DateTimeExtensions
    {
        /// <summary>
        ///             unix base date.
        /// </summary>
        public static readonly DateTime UnixBaseDate = new DateTime(1970, 1, 1);

        /// <summary>
        /// Convert unix time stamp to <see cref="T:System.DateTime"/>.
        /// </summary>
        /// <param name="timestamp">The timestamp.</param>
        /// <returns>A new <see cref="T:System.DateTime"/>.</returns>
        public static DateTime FromUnixTime(double timestamp)
        {
            return UnixBaseDate.AddSeconds(timestamp);
        }

        /// <summary>
        /// Convert unix time stamp to <see cref="T:System.DateTime"/>.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <param name="timestamp"> The timestamp.</param>
        /// <returns> A new <see cref="T:System.DateTime"/>.</returns>
        public static DateTime FromUnixTime(this DateTime dateTime, double timestamp)
        {
            return FromUnixTime(timestamp);
        }
    }
}
