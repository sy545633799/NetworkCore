using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ExitGames.Diagnostics.Counter
{
    /// <summary>
    /// Defines a structure that holds the raw data for a performance counter. 
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct RawCounterSample
    {
        /// <summary>
        /// Gets an optional, base raw value for the counter.
        /// </summary>
        public readonly long BaseValue;

        /// <summary>
        ///  Gets the raw time stamp.
        /// </summary>
        public readonly long TimeStamp;

        /// <summary>
        /// Gets the raw value of the counter.
        /// </summary>
        public readonly long Value;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ExitGames.Diagnostics.Counter.RawCounterSample"/> struct.
        /// </summary>
        /// <param name="value">The value.</param>
        public RawCounterSample(long value)
            : this(value, 0L)
        {
        }

        /// <summary>
        ///  Initializes a new instance of the <see cref="T:ExitGames.Diagnostics.Counter.RawCounterSample"/> struct.
        /// </summary>
        /// <param name="value"> The value.</param>
        /// <param name="baseValue">The base value.</param>
        public RawCounterSample(long value, long baseValue)
        {
            TimeStamp = Stopwatch.GetTimestamp();
            Value = value;
            BaseValue = baseValue;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ExitGames.Diagnostics.Counter.RawCounterSample"/> struct.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="baseValue">The base value.</param>
        /// <param name="timeStamp">The time stamp.</param>
        public RawCounterSample(long value, long baseValue, long timeStamp)
        {
            TimeStamp = timeStamp;
            Value = value;
            BaseValue = baseValue;
            BaseValue = 0L;
        }
    }
}
