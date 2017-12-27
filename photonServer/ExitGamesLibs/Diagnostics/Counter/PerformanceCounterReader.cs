using System;
using System.Diagnostics;

namespace ExitGames.Diagnostics.Counter
{
    /// <summary>
    /// A read only <see cref="T:ExitGames.Diagnostics.Counter.ICounter"/> that wraps a windows <see 
    /// cref="T:System.Diagnostics.PerformanceCounter"/>.
    /// </summary>
    public class PerformanceCounterReader : IDisposable, ICounter
    {
        /// <summary>
        ///  The windows performance counter category.
        /// </summary>
        private PerformanceCounterCategory category;

        /// <summary>
        ///   The windows performance counter.
        /// </summary>
        private PerformanceCounter counter;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ExitGames.Diagnostics.Counter.PerformanceCounterReader"/> class.
        /// </summary>
        /// <param name="categoryName">The name of the performance counter category.</param>
        /// <param name="counterName">The performance counter name counter.</param>
        public PerformanceCounterReader(string categoryName, string counterName)
            : this(categoryName, counterName, string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ExitGames.Diagnostics.Counter.PerformanceCounterReader"/> class.
        /// </summary>
        /// <param name="categoryName">The name of the performance counter category.</param>
        /// <param name="counterName">The performance counter name counter.</param>
        /// <param name="instanceName">The instance name for the performance counter.</param>
        public PerformanceCounterReader(string categoryName, string counterName, string instanceName)
        {
            CategoryName = categoryName;
            Name = counterName;
            InstanceName = instanceName;
            Initialize();
        }

        /// <summary>
        /// Initalizes this instance. 
        /// There is usally no need to call this method since the constructor has already taken care of it.
        /// </summary>
        public void Initialize()
        {
            if (counter != null)
            {
                counter.Dispose();
            }
            counter = null;
            category = null;
            CategoryExists = false;
            CounterExists = false;
            InstanceExists = false;
            if (PerformanceCounterCategory.Exists(CategoryName))
            {
                CategoryExists = true;
                category = new PerformanceCounterCategory(CategoryName);
                IsSingleInstance = category.CategoryType == PerformanceCounterCategoryType.SingleInstance;
                InitializeCounter();
            }
        }

        /// <summary>
        /// Tries to obtain a counter sample and returns the calculated value for it. 
        /// </summary>
        /// <param name="value">When this method returns, contains the calculated value for the performance counter if the the performance counter exists; otherwise, the default value for the type of the value parameter is returned. </param>
        /// <returns>True if performcae counter exists and the calcualted value could be obtained; otherwise, false. </returns>
        public bool TryGetValue(out float value)
        {
            if (CounterExists)
            {
                if (counter == null)
                {
                    InitializeCounter();
                }
                if (counter != null)
                {
                    try
                    {
                        value = counter.NextValue();
                        return true;
                    }
                    catch (InvalidOperationException)
                    {
                        counter = null;
                    }
                }
            }
            value = 0;
            return false;
        }

        /// <summary>
        /// Not supported by this readonly <see cref="T:ExitGames.Diagnostics.Counter.ICounter"/> implementation.
        /// </summary>
        /// <returns>Throws a <see cref="T:System.NotSupportedException"/>.</returns>
        /// <exception cref="T:System.NotSupportedException">
        ///  This counter is read only.
        /// </exception>
        public long Decrement()
        {
            throw new NotSupportedException("Counter instance is readonly");
        }

        /// <summary>
        /// Obtains a counter sample and returns the calculated value for it.
        /// </summary>
        /// <returns>The next calculated value that the system obtains for this counter.</returns>
        public float GetNextValue()
        {
            float value;
            TryGetValue(out value);
            return value;

        }

        /// <summary>
        /// Not supported by this readonly <see cref="T:ExitGames.Diagnostics.Counter.ICounter"/> implementation.
        /// </summary>
        /// <returns>Throws a <see cref="T:System.NotSupportedException"/>.</returns>
        /// <exception cref="T:System.NotSupportedException">
        ///  This counter is read only.
        /// </exception>
        public long Increment()
        {
            throw new NotSupportedException("Counter instance is readonly");
        }

        /// <summary>
        /// Not supported by this readonly <see cref="T:ExitGames.Diagnostics.Counter.ICounter"/> implementation.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>Throws a <see cref="T:System.NotSupportedException"/>.</returns>
        /// <exception cref="T:System.NotSupportedException">
        /// This counter is read only.
        /// </exception>
        public long IncrementBy(long value)
        {
            throw new NotSupportedException("Counter instance is readonly");
        }

        /// <summary>
        /// Releases all resources used by this instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the wrapped <see cref="T:System.Diagnostics.PerformanceCounter"/>.
        /// </summary>
        /// <param name="disposing">The disposing.</param>
        private void Dispose(bool disposing)
        {
            if (disposing && (counter != null))
            {
                counter.Dispose();
            }
        }

        /// <summary>
        /// Initializes the counter.
        /// </summary>
        private void InitializeCounter()
        {
            if (category != null)
            {
                CounterExists = category.CounterExists(Name);
                if (CounterExists)
                {
                    if (IsSingleInstance)
                    {
                        counter = new PerformanceCounter(CategoryName, Name, true);
                    }
                    else
                    {
                        InstanceExists = category.InstanceExists(InstanceName);
                        if (InstanceExists)
                        {
                            counter = new PerformanceCounter(CategoryName, Name, InstanceName, true);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the given performance counter category exists. 
        /// </summary>
        public bool CategoryExists { get; private set; }

        /// <summary>
        /// Gets the name of the performance counter category for the performance counter. 
        /// </summary>
        public string CategoryName { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the given performance counter exists. 
        /// </summary>
        public bool CounterExists { get; private set; }

        /// <summary>
        /// Gets the counter type of the associated performance counter. 
        /// </summary>
        public CounterType CounterType { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the given performance counter instance exists. 
        /// </summary>
        public bool InstanceExists { get; private set; }

        /// <summary>
        /// Gets the instance name for the performance counter. 
        /// </summary>
        public string InstanceName { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the performance counter category can have only a single instance. 
        /// </summary>
        public bool IsSingleInstance { get; private set; }

        /// <summary>
        /// Gets the name of the counter. 
        /// </summary>
        public string Name { get; private set; }
    }
}
