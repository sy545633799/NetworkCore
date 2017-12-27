using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExitGames.Logging;

namespace ExitGames.Diagnostics.Counter
{
    /// <summary>
    /// The base class for counters. 
    /// </summary>
    public abstract class CounterBase : ICounter
    {
        /// <summary>
        /// Provides a <see cref="T:ExitGames.Logging.ILogger"/> instance used to log messages into the logging framework.
        /// </summary>
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ExitGames.Diagnostics.Counter.CounterBase"/> class.
        /// </summary>
        protected CounterBase()
        {
            Name = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ExitGames.Diagnostics.Counter.CounterBase"/> class. 
        /// </summary>
        /// <param name="name">The counter name. </param>
        protected CounterBase(string name)
        {
            if (log.IsDebugEnabled)
            {
                log.DebugFormat("Creating {0}: Name='{1}'", this.GetType().Name, name);
            }
            Name = name;
        }

        /// <summary>
        /// Decrements the counter. 
        /// </summary>
        /// <returns>The decremented value. </returns>
        public abstract long Decrement();

        /// <summary>
        /// Gets the next sample. 
        /// </summary>
        /// <returns>The next sample. </returns>
        public abstract RawCounterSample GetNextSample();

        /// <summary>
        /// Gets the next value. 
        /// </summary>
        /// <returns>The next value. </returns>
        public abstract  float GetNextValue()             ;

       /// <summary>
        /// Increments the counter. 
       /// </summary>
       /// <returns>The incremented counter. </returns>
        public abstract long Increment();

        /// <summary>
        /// Increments the counter by a value. 
        /// </summary>
        /// <param name="value">The value. </param>
        /// <returns>The incremented counter. </returns>
        public abstract long IncrementBy(long value);

        /// <summary>
        /// Gets the type of the counter. 
        /// </summary>
        /// <value>The type of the counter.</value>
        public abstract CounterType CounterType { get; }

        /// <summary>
        /// Gets Name. 
        /// </summary>
        public string Name
        {
            get;
            private set;
        }
    }
}
