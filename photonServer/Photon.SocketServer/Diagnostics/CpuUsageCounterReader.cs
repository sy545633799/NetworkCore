using System;
using System.Diagnostics;
using ExitGames.Diagnostics.Counter;
using ExitGames.Diagnostics.Monitoring;

namespace Photon.SocketServer.Diagnostics
{
    /// <summary>
    /// The process cpu usage counter. 
    /// </summary>
    public sealed class CpuUsageCounterReader : ICounter
    {
        /// <summary>
        /// The windows performance counter field. 
        /// </summary>
        private readonly ICounter windowsPerformanceCounterField = CounterFactory.TryCreateWindowsCounter("CpuPhoton", "Process", "% Processor Time", Process.GetCurrentProcess().ProcessName);

        /// <summary>
        /// This method is not supported.
        /// </summary>
        /// <returns>
        ///  Nothing. Throws a <see cref="T:System.NotSupportedException"/>.
        /// </returns>
        /// <exception cref="T:System.NotSupportedException">
        /// This is a read only counter.
        ///</exception>
        public long Decrement()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Gets the next value.
        /// </summary>
        /// <returns>
        ///  The next value.
        /// </returns>
        public float GetNextValue()
        {
            return (this.windowsPerformanceCounterField.GetNextValue() / ((float)Environment.ProcessorCount));
        }

        /// <summary>
        /// This method is not supported.
        /// </summary>
        /// <returns>
        ///  Nothing. Throws a <see cref="T:System.NotSupportedException"/>.
        /// </returns>
        /// <exception cref="T:System.NotSupportedException">
        /// This is a read only counter.
        ///</exception>
        public long Increment()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        ///  This method is not supported.
        /// </summary>
        /// <param name="value">
        /// The value to increment by.
        /// </param>
        /// <returns>
        /// Nothing. Throws a <see cref="T:System.NotSupportedException"/>.
        /// </returns>
        /// <exception cref="T:System.NotSupportedException">
        /// This is a read only counter.
        /// </exception>
        public long IncrementBy(long value)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Gets CounterType.
        /// </summary>
        public CounterType CounterType
        {
            get
            {
                return this.windowsPerformanceCounterField.CounterType;
            }
        }

        /// <summary>
        /// Gets Name.
        /// </summary>
        public string Name
        {
            get
            {
                return this.windowsPerformanceCounterField.Name;
            }
        }
    }
}
