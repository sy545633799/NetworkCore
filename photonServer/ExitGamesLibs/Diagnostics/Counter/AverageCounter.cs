
namespace ExitGames.Diagnostics.Counter
{
    /// <summary>
    /// The average counter. 
    /// </summary>
    /// <remarks>
    /// <see cref="M:ExitGames.Diagnostics.Counter.AverageCounter.GetNextValue"/> calculates the average counter since the previous <see
    /// cref="M:ExitGames.Diagnostics.Counter.AverageCounter.GetNextValue"/> call.
    /// </remarks>
    public class AverageCounter : CounterBase
    {
        /// <summary>
        ///  _shared counter.
        /// </summary>
        private readonly SharedCounter sharedCounter;

        /// <summary>
        /// _shared counter base.
        /// </summary>
        private readonly SharedCounter sharedCounterBase;

        /// <summary>
        /// _old sample.
        /// </summary>
        private RawCounterSample oldSample;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ExitGames.Diagnostics.Counter.AverageCounter"/> class.
        /// </summary>
        public AverageCounter()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ExitGames.Diagnostics.Counter.AverageCounter"/> class.
        /// </summary>
        /// <param name="name">The counter name.</param>
        public AverageCounter(string name)
            : base(name)
        {
            this.sharedCounter = new SharedCounter(name);
            this.sharedCounterBase = new SharedCounter(name);
            this.oldSample = this.GetNextSample();
        }

        /// <summary>
        /// Decrements the counter. 
        /// </summary>
        /// <returns>The decremented counter. </returns>
        public override long Decrement()
        {
            this.sharedCounterBase.Decrement();
            return this.sharedCounter.Decrement();
        }

        /// <summary>
        /// Gets the next sample. 
        /// </summary>
        /// <returns>A new <see cref="T:ExitGames.Diagnostics.Counter.RawCounterSample"/>.</returns>
        ///<remarks>This method is not thread safe.</remarks>
        public sealed override RawCounterSample GetNextSample()
        {
            return new RawCounterSample(this.sharedCounter.Value, this.sharedCounterBase.Value);
        }

        /// <summary>
        /// Returns the average count since the last <see
        /// cref="M:ExitGames.Diagnostics.Counter.AverageCounter.GetNextValue"/> call.
        /// </summary>
        /// <returns>The new average value. </returns>
        /// <remarks>This method is NOT thread safe. </remarks>
        public override float GetNextValue()
        {
            RawCounterSample sample = this.oldSample;
            RawCounterSample nextSample = this.GetNextSample();
            this.oldSample = nextSample;
            long num = nextSample.BaseValue - sample.BaseValue;
            long num2 = nextSample.Value - sample.Value;
            if (num == 0L)
            {
                return 0f;
            }
            return (float)num2 / (float)num;
        }

        /// <summary>
        /// Increments the counter. 
        /// </summary>
        /// <returns>The incremented counter. </returns>
        public override long Increment()
        {
            this.sharedCounterBase.Increment();
            return this.sharedCounter.Increment();
        }

        /// <summary>
        /// Increments the counter by a value. 
        /// </summary>
        /// <param name="value">The value. </param>
        /// <returns>The incremented counter. </returns>
        public override long IncrementBy(long value)
        {
            this.sharedCounterBase.Increment();
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
                return CounterType.Average;
            }
        }
    }
}
