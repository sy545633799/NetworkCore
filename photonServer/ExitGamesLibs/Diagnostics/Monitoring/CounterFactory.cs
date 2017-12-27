using System;
using System.Diagnostics;
using System.Threading;
using ExitGames.Diagnostics.Counter;
using ExitGames.Logging;

namespace ExitGames.Diagnostics.Monitoring
{
    /// <summary>
    /// Provides methods to create <see cref="T:ExitGames.Diagnostics.Counter.ICounter"/> instances.
    /// </summary>
    public static class CounterFactory
    {
        /// <summary>
        /// Provides a <see cref="T:ExitGames.Logging.ILogger"/> instance used to log messages into the logging framework.
        /// </summary>
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Log an error.
        /// </summary>
        /// <param name="performanceCounterCategory">The performance counter category.</param>
        /// <param name="performanceCounterName">The performance counter name.</param>
        /// <param name="performanceCounterInstance">The performance counter instance.</param>
        /// <param name="errorMessage">The error message.</param>
        private static void LogPerformanceCounterError(string performanceCounterCategory, string performanceCounterName, string performanceCounterInstance, string errorMessage)
        {
            log.WarnFormat("Failed to create performance counter: Category='{0}', Name='{1}', Instance='{2}', Error='{3}'", performanceCounterCategory, performanceCounterName, performanceCounterInstance, errorMessage);
        }

        /// <summary>
        /// Tries the create windows counter.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="performanceCounterCategory">The performance counter category.</param>
        /// <param name="performanceCounterName">Nam of the perormance counter.</param>
        /// <returns>If the performance counter creation succeds an instance of <see
        /// cref="T:ExitGames.Diagnostics.Counter.WindowsPerformanceCounter"/> 
        /// is returned, otherwise an instance of <see 
        /// cref="T:ExitGames.Diagnostics.Counter.DummyCounter"/> is returned.</returns>
        public static ICounter TryCreateWindowsCounter(string name, string performanceCounterCategory, string performanceCounterName)
        {
            return TryCreateWindowsCounter(name, performanceCounterCategory, performanceCounterName, string.Empty);
        }

        /// <summary>
        /// Tries to create a <see cref="T:ExitGames.Diagnostics.Counter.ICounter"/> wrapper for an <see cref="T:System.Diagnostics.PerformanceCounter"/>.
        /// </summary>
        /// <param name="name">The counter name.</param>
        /// <param name="performanceCounterCategory">The name of the performance counter category (performance object) with which this performance counter is associated.</param>
        /// <param name="performanceCounterName">The name of the performance counter.</param>
        /// <param name="performanceCounterInstance">The name of the performance counter category instance, or an empty string (""), if the category contains a single instance.</param>
        /// <returns>If the performance counter creation succeds an instance of <see 
        /// cref="T:ExitGames.Diagnostics.Counter.WindowsPerformanceCounter"/> 
        /// is returned, otherwise an instance of <see
        /// cref="T:ExitGames.Diagnostics.Counter.DummyCounter"/> is returned.</returns>
        public static ICounter TryCreateWindowsCounter(string name, string performanceCounterCategory, string performanceCounterName, string performanceCounterInstance)
        {
            ICounter counter2;
            try
            {
                if (!PerformanceCounterCategory.Exists(performanceCounterCategory))
                {
                    if (log.IsWarnEnabled)
                    {
                        LogPerformanceCounterError(performanceCounterCategory, performanceCounterName, performanceCounterInstance, "Performance counter category does not exists.");
                    }
                    return new DummyCounter(name);
                }
                PerformanceCounter counter = new PerformanceCounter(performanceCounterCategory, performanceCounterName, performanceCounterInstance, true);
                counter2 = new WindowsPerformanceCounter(counter);
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (OutOfMemoryException)
            {
                throw;
            }
            catch (Exception exception)
            {
                if (log.IsWarnEnabled)
                {
                    LogPerformanceCounterError(performanceCounterCategory, performanceCounterName, performanceCounterInstance, exception.Message);
                }
                counter2 = new DummyCounter(name);
            }
            return counter2;
        }
    }
}
