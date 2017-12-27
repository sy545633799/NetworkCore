using System.Threading;

namespace ExitGames.Diagnostics.Counter
{
    /// <summary>
    /// shared counter.
    /// </summary>
    internal class SharedCounter
    {
        /// <summary>
        /// The counter name.
        /// </summary>
        private readonly string name;

        /// <summary>
        /// The counter value.
        /// </summary>
        private long value;

        /// <summary>
        /// Initializes a new instance of the <see 
        /// cref="T:ExitGames.Diagnostics.Counter.Internal.SharedCounter"/> class.
        /// </summary>
        /// <param name="name">The counter name.</param>
        public SharedCounter(string name)
        {
            this.name = name;
            value = 0;
        }

        /// <summary>
        /// Decrements the counter by one.
        /// </summary>
        /// <returns>The decremented value.</returns>
        public long Decrement()
        {
            return Interlocked.Decrement(ref  value);
        }

        public long Value
        {
            get { return Interlocked.Read(ref value); }
            set { Interlocked.Exchange(ref value, value); }
        }

        /// <summary>
        /// Increments the counter by one and returns the new value.
        /// </summary>
        /// <returns>The incremented value.</returns>
        public long Increment()
        {
            return Interlocked.Increment(ref value);
        }

        /// <summary>
        /// Increments the value by the given value.
        /// </summary>
        /// <param name="value"> The value to be added to the counter.</param>
        /// <returns>The incremented value.</returns>
        public long IncrementBy(ref long valueToAdd)
        {
            return Interlocked.Add(ref value, valueToAdd);
        }
    }
}
