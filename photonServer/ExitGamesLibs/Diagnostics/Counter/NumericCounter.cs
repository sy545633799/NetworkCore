namespace ExitGames.Diagnostics.Counter
{
    /// <summary>
    /// A numeric counter. 
    /// </summary>
    public class NumericCounter : CounterBase
    {
        /// <summary>
        /// shared counter.
        /// </summary>
        private readonly SharedCounter sharedCounter;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ExitGames.Diagnostics.Counter.NumericCounter"/> class.
        /// </summary>
        public NumericCounter()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ExitGames.Diagnostics.Counter.NumericCounter"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public NumericCounter(string name)
            : base(name)
        {
            this.sharedCounter = new SharedCounter(name);
        }

        /// <summary>
        /// Decrements the counter by one. 
        /// </summary>
        /// <returns>The decremented value. </returns>
        public override long Decrement()
        {
            return this.sharedCounter.Decrement();
        }

        /// <summary>
        /// Gets the next sample.
        /// </summary>
        /// <returns>A new <see cref="T:ExitGames.Diagnostics.Counter.RawCounterSample"/>.</returns>
        public override RawCounterSample GetNextSample()
        {
            return new RawCounterSample(this.RawValue);
        }

        /// <summary>
        /// Gets the next value. 
        /// </summary>
        /// <returns>The get next value.</returns>
        public override float GetNextValue()
        {
            return (float)this.RawValue;
        }

        /// <summary>
        /// Increments the counter by one and returns the new value. 
        /// </summary>
        /// <returns>The incremented value. </returns>
        public override long Increment()
        {
            return this.sharedCounter.Increment();
        }

        /// <summary>
        /// Increments the counter by a given value. s
        /// </summary>
        /// <param name="value">The value to be added to the counter. </param>
        /// <returns>The incremented value. </returns>
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
                return CounterType.Numeric;
            }
        }

        /// <summary>
        /// Gets or sets RawValue. 
        /// </summary>
        public long RawValue
        {
            get
            {
                return this.sharedCounter.Value;
            }
            set
            {
                this.sharedCounter.Value = value;
            }
        }
    }
}
