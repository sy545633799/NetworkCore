namespace ExitGames.Diagnostics.Counter
{
    /// <summary>
    /// A dummy counter that does nothing. 
    /// </summary>
    public class DummyCounter : CounterBase
    {
         /// <summary>
        /// Initializes a new instance of the <see cref="T:ExitGames.Diagnostics.Counter.DummyCounter"/> class.
         /// </summary>
         /// <param name="name"></param>
        public DummyCounter(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Decrements the counter.  
        /// </summary>
        /// <returns>Always zero. </returns>
        public override long Decrement()
        {
            return 0L;
        }

        /// <summary>
        /// Gets the next sample. 
        /// </summary>
        /// <returns>A new <see cref="T:ExitGames.Diagnostics.Counter.RawCounterSample"/>.</returns>
        public override RawCounterSample GetNextSample()
        {
            return new RawCounterSample();
        }

        /// <summary>
        /// Gets the next value. 
        /// </summary>
        /// <returns>Always zero. </returns>
        public override float GetNextValue()
        {
            return 0f;
        }

        /// <summary>
        /// Increments the counter. 
        /// </summary>
        /// <returns>Always zero. </returns>
        public override long Increment()
        {
            return 0L;
        }

        /// <summary>
        /// Increments the counter. 
        /// </summary>
        /// <param name="value">The value. </param>
        /// <returns>Always zero. </returns>
        public override long IncrementBy(long value)
        {
            return 0L;
        }

        /// <summary>
        /// Gets the type of the counter. 
        /// </summary>
        /// <value>The type of the counter</value>
        public override CounterType CounterType
        {
            get
            {
                return CounterType.Undefined;
            }
        }
    }
}
