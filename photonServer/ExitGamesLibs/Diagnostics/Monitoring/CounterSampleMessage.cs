using System;
using ExitGames.Diagnostics.Counter;

namespace ExitGames.Diagnostics.Monitoring
{
    /// <summary>
    /// A message used by the <see cref="T:ExitGames.Diagnostics.Monitoring.CounterSamplePublisher"/>.
    /// </summary>
    public class CounterSampleMessage
    {
        /// <summary>
        /// Gets the name of the counter.
        /// </summary>
        public readonly string CounterName;

        /// <summary>
        /// Gets the <see cref="F:ExitGames.Diagnostics.Monitoring.CounterSampleMessage.CounterSample"/>.
        /// </summary>
        public readonly CounterSample CounterSample;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ExitGames.Diagnostics.Monitoring.CounterSampleMessage"/> class.
        /// </summary>
        /// <param name="counterName">The name of the counter.</param>
        /// <param name="counterSample">The calculated value of the counter.</param>
        public CounterSampleMessage(string counterName, CounterSample counterSample)
        {
            this.CounterName = counterName;
            this.CounterSample = counterSample;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ExitGames.Diagnostics.Monitoring.CounterSampleMessage"/> class.
        /// </summary>
        /// <param name="counterName">The name of the counter.</param>
        /// <param name="value">>The calculated value of the counter.</param>
        public CounterSampleMessage(string counterName, float value)
            : this(counterName, DateTime.UtcNow, value)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ExitGames.Diagnostics.Monitoring.CounterSampleMessage"/> class.
        /// </summary>
        /// <param name="counterName">The name of the counter.</param>
        /// <param name="timestamp">The timestamp when the value has been taken from the counter.</param>
        /// <param name="value">The value.</param>
        public CounterSampleMessage(string counterName, DateTime timestamp, float value)
            : this(counterName, new CounterSample(timestamp, value))
        {
        }
    }
}
