using System;
using System.Runtime.InteropServices;

namespace ExitGames.Diagnostics.Counter
{
    /// <summary>
    /// Represents the value of a counter calculated at a specific time. 
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CounterSample
    {
        /// <summary>
        /// Gets the timestamp of the counter sample. 
        /// </summary>
        public readonly DateTime Timestamp;

        /// <summary>
        /// Gets the calculated value of the counter. 
        /// </summary>
        public readonly float Value;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ExitGames.Diagnostics.Counter.CounterSample"/> struct. 
        /// The <see cref="F:ExitGames.Diagnostics.Counter.CounterSample.Timestamp"/> property is set to <see
        /// cref="P:System.DateTime.UtcNow"/>.
        /// </summary>
        /// <param name="value">The calculated value of the counter. </param>
        public CounterSample(float value) : this(DateTime.UtcNow, value) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ExitGames.Diagnostics.Counter.CounterSample"/> struct.
        /// </summary>
        /// <param name="timestamp">The timestamp of the counter sample. </param>
        /// <param name="value">The calculated value of the counter. </param>
        public CounterSample(DateTime timestamp, float value)
        {
            this.Timestamp = timestamp;
            this.Value = value;
        }
    }
}
