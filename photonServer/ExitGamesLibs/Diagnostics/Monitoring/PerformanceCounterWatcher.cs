using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using ExitGames.Logging;

namespace ExitGames.Diagnostics.Monitoring
{
    /// <summary>
    /// Delegate for new performance counter instances. 
    /// </summary>
    /// <param name="instanceName">The counter instance name. </param>
    public delegate void PerformanceCounterInstancesDelegate(string instanceName);

    /// <summary>
    /// This class monitors the windows performance counters for new instances.
    /// </summary>
    public class PerformanceCounterWatcher
    {
        /// <summary>
        /// The logger.
        /// </summary>
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The windows performance counter category.
        /// </summary>
        private readonly PerformanceCounterCategory category;

        /// <summary>
        /// The windows performance counter category name.
        /// </summary>
        private readonly string categoryName;

        /// <summary>
        /// The reg ex for counters.
        /// </summary>
        private readonly Regex regEx;

        /// <summary>
        /// The counter instances.
        /// </summary>
        private HashSet<string> instances;

        /// <summary>
        /// The update timer.
        /// </summary>
        private Timer timer;

        /// <summary>
        /// The update interval.
        /// </summary>
        private int updateInterval = 100;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ExitGames.Diagnostics.Monitoring.PerformanceCounterWatcher"/> class.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="instancePattern">The instance pattern.</param>
        public PerformanceCounterWatcher(string category, string instancePattern)
        {
            this.categoryName = category;
            this.category = new PerformanceCounterCategory(this.categoryName);
            this.regEx = new Regex(instancePattern);
            this.instances = new HashSet<string>();
            this.Update();
        }

        /// <summary>
        /// Starts the watcher.
        /// </summary>
        /// <param name="updateIntervalTime">The update interval.</param>
        public void Start(int updateIntervalTime)
        {
            this.updateInterval = updateIntervalTime;
            this.timer = new Timer(new TimerCallback(this.TimerCallback), null, 0, Timeout.Infinite);
        }

        /// <summary>
        /// This method reads all instances.
        /// </summary>
        public void Update()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            string[] strArray = this.category.GetInstanceNames();
            HashSet<string> set = new HashSet<string>();
            foreach (string str in strArray)
            {
                if (this.instances.Contains(str))
                {
                    set.Add(str);
                }
                else if (this.regEx.IsMatch(str))
                {
                    if (log.IsDebugEnabled)
                    {
                        log.DebugFormat("Found new instance match: {0}", str);
                    }
                    set.Add(str);
                    if (this.NewInstances != null)
                    {
                        this.NewInstances(str);
                    }
                }
            }
            foreach (string str2 in this.instances)
            {
                if (!set.Contains(str2))
                {
                    if (log.IsDebugEnabled)
                    {
                        log.DebugFormat("Instance removed : {0}", str2);
                    }
                    if (this.InstancesRemoved != null)
                    {
                        this.InstancesRemoved(str2);
                    }
                }
            }
            this.instances = set;
            stopwatch.Stop();
            if (log.IsDebugEnabled)
            {
                log.DebugFormat("Update took {0}ms.", stopwatch.Elapsed.TotalMilliseconds);
            }
        }

        /// <summary>
        /// timer callback.
        /// </summary>
        /// <param name="state">The state.</param>
        private void TimerCallback(object state)
        {
            try
            {
                this.Update();
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
                log.Error(exception);
            }
            finally
            {
                this.timer.Change(this.updateInterval, Timeout.Infinite);
            }
        }

        /// <summary>
        /// Invoked when a windows performance counter instances are removed.
        /// </summary>
        public event PerformanceCounterInstancesDelegate InstancesRemoved;

        /// <summary>
        /// Invoked when new windows performance counter instances are added.
        /// </summary>
        public event PerformanceCounterInstancesDelegate NewInstances;

    }
}
