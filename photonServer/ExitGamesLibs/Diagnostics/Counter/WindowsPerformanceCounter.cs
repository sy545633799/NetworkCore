using System.Diagnostics;

namespace ExitGames.Diagnostics.Counter
{
    /// <summary>
    /// An <see cref="T:ExitGames.Diagnostics.Counter.ICounter"/> that wraps a windows <see cref="T:System.Diagnostics.PerformanceCounter"/>.
    /// </summary>
    public class WindowsPerformanceCounter : ICounter
    {
        /// <summary>
        /// The windows performance counter.
        /// </summary>
        private readonly PerformanceCounter counter;

        /// <summary>
        /// The counter name.
        /// </summary>
        private readonly string name;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ExitGames.Diagnostics.Counter.WindowsPerformanceCounter"/> class.
        /// </summary>
        /// <param name="counter">The counter.</param>
        public WindowsPerformanceCounter(PerformanceCounter counter)
        {
            name = counter.CounterName;
            this.counter = counter;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ExitGames.Diagnostics.Counter.WindowsPerformanceCounter"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="counter">The counter.</param>
        public WindowsPerformanceCounter(string name, PerformanceCounter counter)
        {
            this.name = name;
            this.counter = counter;
        }

        /// <summary>
        /// Create a new instance of <see cref="T:ExitGames.Diagnostics.Counter.WindowsPerformanceCounter"/>.
        /// </summary>
        /// <param name="name">The counter.</param>
        /// <param name="categoryName">The windows performance counter category name.</param>
        /// <param name="counterName">The windows performance counter name.</param>
        /// <returns>A new instance of <see cref="T:ExitGames.Diagnostics.Counter.WindowsPerformanceCounter"/>.</returns>
        public static WindowsPerformanceCounter CreateCounter(string name, string categoryName, string counterName)
        {
            return new WindowsPerformanceCounter(name, new PerformanceCounter(categoryName, counterName, true));
        }

        /// <summary>
        /// Create a new instance of <see cref="T:ExitGames.Diagnostics.Counter.WindowsPerformanceCounter"/>.
        /// </summary>
        /// <param name="name">The counter.</param>
        /// <param name="categoryName">The windows performance counter category name.</param>
        /// <param name="counterName">The windows performance counter name.</param>
        /// <param name="instanceName">The windows performance counter instance Name.</param>
        /// <returns>A new instance of <see cref="T:ExitGames.Diagnostics.Counter.WindowsPerformanceCounter"/>.</returns>
        public static WindowsPerformanceCounter CreateCounter(string name, string categoryName, string counterName, string instanceName)
        {
            return new WindowsPerformanceCounter(name, new PerformanceCounter(categoryName, counterName, instanceName, true));
        }

        /// <summary>
        /// Decrements the counter.
        /// </summary>
        /// <returns>The new value.</returns>
        public long Decrement()
        {
            return counter.Decrement();
        }

        /// <summary>
        /// Gets the next value.
        /// </summary>
        /// <returns>The next value.</returns>
        public float GetNextValue()
        {
            return counter.NextValue();
        }

        /// <summary>
        /// Increments the counter.
        /// </summary>
        /// <returns>The new value.</returns>
        public long Increment()
        {
            return counter.Increment();
        }

        /// <summary>
        /// Increments the counter by a value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The new value.</returns>
        public long IncrementBy(long value)
        {
            return counter.IncrementBy(value);
        }

        /// <summary>
        /// Gets the type of the counter.
        /// </summary>
        /// <value>The type of the counter.</value>
        public CounterType CounterType
        {
            get
            {
                return CounterType.WindowsPerformanceCounter;
            }
        }

        /// <summary>
        /// Gets the counter name.
        /// </summary>
        public string Name
        {
            get
            {
                return name;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="P:System.Diagnostics.PerformanceCounter.RawValue"/>.
        /// </summary>
        public long Value
        {
            get
            {
                return counter.RawValue;
            }
            set
            {
                counter.RawValue = value;
            }
        }
    }
}
