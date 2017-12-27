using System.Diagnostics;

namespace ExitGames.Diagnostics.Counter
{
    /// <summary>
    /// The counts per second rate counter. 
    /// </summary>
    public class CountsPerSecondCounter : CounterBase
    {
        /// <summary>
        /// _shared counter.
        /// </summary>
        private readonly SharedCounter sharedCounter;

        /// <summary>
        /// _old sample.
        /// </summary>
        private RawCounterSample oldSample;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ExitGames.Diagnostics.Counter.CountsPerSecondCounter"/> class.
        /// </summary>
        public CountsPerSecondCounter()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ExitGames.Diagnostics.Counter.CountsPerSecondCounter"/> class.
        /// </summary>
        /// <param name="counterName">The counterName.</param>
        public CountsPerSecondCounter(string counterName)
            : base(counterName)
        {
            this.sharedCounter = new SharedCounter(counterName);
            this.oldSample = this.GetNextSample();
        }

        /// <summary>
        /// Decrements the sharedCounter.
        /// </summary>
        /// <returns>The new value.</returns>
        public override long Decrement()
        {
            return this.sharedCounter.Decrement();
        }

        /// <summary>
        ///  Gets the next sample.
        /// </summary>
        /// <returns>A new <see cref="T:ExitGames.Diagnostics.Counter.RawCounterSample"/>.</returns>
        public sealed override RawCounterSample GetNextSample()
        {
            return new RawCounterSample(this.sharedCounter.Value);
        }

        /// <summary>
        /// Get the value since the last <see cref="M:ExitGames.Diagnostics.Counter.CountsPerSecondCounter.GetNextValue"/> call.
        /// </summary>
        /// <returns>A new <see cref="T:ExitGames.Diagnostics.Counter.RawCounterSample"/>.</returns>
        /// <remarks>This method is NOT thread safe.</remarks>
        public override float GetNextValue()
        {
            RawCounterSample sample = this.oldSample;
            RawCounterSample nextSample = this.GetNextSample();
            this.oldSample = nextSample;
            long num = nextSample.TimeStamp - sample.TimeStamp;
            long num2 = nextSample.Value - sample.Value;
            if (num == 0L)
            {
                return 0f;
            }
            return (float)num2 / (float)num / (float)Stopwatch.Frequency;
        }

        /// <summary>
        /// Increments the counter.
        /// </summary>
        /// <returns>The new value. </returns>
        public override long Increment()
        {
            return this.sharedCounter.Increment();
        }

        /// <summary>
        /// Increment the counter by a value. 
        /// </summary>
        /// <param name="value">The value. </param>
        /// <returns>The new value. </returns>
        public override long IncrementBy(long value)
        {
            return this.sharedCounter.IncrementBy(ref value);
        }

        /// <summary>
        /// Gets the type of the counter. 
        /// </summary>
        /// <value>The type of the counter.</value>
        public override CounterType CounterType
        {
            get
            {
                return CounterType.CountPerSecound;
            }
        }
    }
}
