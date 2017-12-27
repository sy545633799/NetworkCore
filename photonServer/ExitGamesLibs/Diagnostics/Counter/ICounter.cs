using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExitGames.Diagnostics.Counter
{
    /// <summary>
    /// Represents a counter instance. 
    /// </summary>
    public interface ICounter
    {
        /// <summary>
        /// Decrements the counter by one. 
        /// </summary>
        /// <returns>The decremented value. </returns>
        long Decrement();

        /// <summary>
        /// Gets the next value. 
        /// </summary>
        /// <returns>The get next value.</returns>
        float GetNextValue();

        /// <summary>
        /// Increments the counter by one and returns the new value. 
        /// </summary>
        /// <returns>The incremented value. </returns>
        long Increment();

        /// <summary>
        /// Increments the counter by a given value. 
        /// </summary>
        /// <param name="value">The value to be added to the counter. </param>
        /// <returns>The incremented value. </returns>
        long IncrementBy(long value);

        /// <summary>
        /// Gets the type of the counter. 
        /// </summary>
        /// <value>The type of the counter.</value>
        CounterType CounterType { get; }

        /// <summary>
        /// Gets the name of the counter. 
        /// </summary>
        string Name { get; }
    }
}
