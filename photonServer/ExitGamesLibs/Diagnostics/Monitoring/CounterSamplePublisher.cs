using System;
using System.Collections.Generic;
using System.Threading;
using ExitGames.Concurrency.Channels;
using ExitGames.Diagnostics.Counter;
using ExitGames.Logging;

namespace ExitGames.Diagnostics.Monitoring
{
    /// <summary>
    /// Collects and publishes data from <see cref="T:ExitGames.Diagnostics.Counter.ICounter"/> instances in a specified interval
    /// </summary>
    public class CounterSamplePublisher
    {
        /// <summary>
        /// A channel for <see cref="T:ExitGames.Diagnostics.Monitoring.CounterSampleMessage"/>s.
        /// </summary>
        public readonly Channel<CounterSampleMessage> Channel = new Channel<CounterSampleMessage>();

        /// <summary>
        /// A channel for lists of <see cref="T:ExitGames.Diagnostics.Monitoring.CounterSampleMessage"/>s.
        /// </summary>
        public readonly Channel<IList<CounterSampleMessage>> ListChannel = new Channel<IList<CounterSampleMessage>>();

        /// <summary>
        /// Provides a <see cref="T:ExitGames.Logging.ILogger"/> instance used to log messages into the logging framework.
        /// </summary>
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// A list of <see 
        /// cref="T:ExitGames.Diagnostics.Monitoring.CounterSamplePublisher.CounterPublisherItem"/>s.
        /// </summary>
        private readonly List<CounterPublisherItem> items;

        /// <summary>
        /// The publish interval in seconds.
        /// </summary>
        private readonly int publishInterval = 1;

        /// <summary>
        /// A sync root.
        /// </summary>
        private readonly object syncRoot = new object();

        /// <summary>
        /// The last update time.
        /// </summary>
        private DateTime lastUpdateDate = DateTime.MinValue;

        /// <summary>
        /// The time for updates.
        /// </summary>
        private Timer timer;

        /// <summary>
        ///  Initializes a new instance of the <see 
        ///  cref="T:ExitGames.Diagnostics.Monitoring.CounterSamplePublisher"/> class.
        /// </summary>
        /// <param name="publishInterval">Specifies the interval to publish counter values in seconds.</param>   
        public CounterSamplePublisher(int publishInterval)
        {
            this.publishInterval = publishInterval;
            this.items = new List<CounterPublisherItem>();
        }

        /// <summary>
        /// Adds an <see cref="T:ExitGames.Diagnostics.Counter.ICounter"/> instance to the counter publisher.
        /// </summary>
        /// <param name="counter">The counter to add.</param>
        /// <param name="name">The name of the counter.</param>
        public void AddCounter(ICounter counter, string name)
        {
            CounterPublisherItem item = new CounterPublisherItem(counter, name);
            lock (syncRoot)
            {
                this.items.Add(item);
            }
        }

        /// <summary>
        /// Starts this instance to collect and publish counter data.
        /// </summary>
        public void Start()
        {
            this.timer = new Timer(new TimerCallback(this.TimerCallBack));
            this.SetTimer(DateTime.UtcNow);
        }

        /// <summary>
        ///  Stops publishing counter data.
        /// </summary>
        public void Stop()
        {
            timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        /// <summary>
        /// Sets the timer to the next update time.
        /// </summary>
        /// <param name="time1"> The last update time.</param>
        private void SetTimer(DateTime lastUpdateDateTime)
        {
            DateTime time = lastUpdateDateTime.AddSeconds((double)this.publishInterval);
            DateTime time2 = DateTime.UtcNow;
            int totalMilliseconds = (int)time.Subtract(time2).TotalMilliseconds;
            totalMilliseconds -= time2.Millisecond;
            if (totalMilliseconds < 0)
            {
                totalMilliseconds = 1000 - time2.Millisecond;
            }
            try
            {
                timer.Change(totalMilliseconds + 10, Timeout.Infinite);
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
        }

        /// <summary>
        /// The timer elapsed callback.
        ///   Calls <see cref="M:ExitGames.Diagnostics.Monitoring.CounterSamplePublisher.Update"/>.
        /// </summary>
        /// <param name="state">The state (is not used).</param>
        private void TimerCallBack(object state)
        {
            try
            {
                this.lastUpdateDate = DateTime.UtcNow;
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
                if (log.IsErrorEnabled)
                {
                    log.Error("Error during update.", exception);
                }
            }
            finally
            {
                this.SetTimer(this.lastUpdateDate);
            }
        }

        /// <summary>
        ///  Reads all counters and publishes them into the <see 
        ///  cref="F:ExitGames.Diagnostics.Monitoring.CounterSamplePublisher.Channel"/> and the <see 
        ///  cref="F:ExitGames.Diagnostics.Monitoring.CounterSamplePublisher.ListChannel"/>.
        /// </summary>
        private void Update()
        {
            CounterSamplePublisher.CounterPublisherItem[] CounterPublisherItemArray;

            List<CounterSampleMessage> msg = new List<CounterSampleMessage>(this.items.Count);
            DateTime currentTimeStamp = DateTime.UtcNow;
            lock (syncRoot)
            {
                CounterPublisherItemArray = this.items.ToArray();
            }
            foreach (CounterSamplePublisher.CounterPublisherItem CounterPublisherItem in CounterPublisherItemArray)
            {
                CounterSample sample;
                if (CounterPublisherItem.TryGetNextValue(currentTimeStamp, out sample))
                {
                    CounterSampleMessage message = new CounterSampleMessage(CounterPublisherItem.Name, sample);
                    this.Channel.Publish(message);
                    msg.Add(message);
                }
            }
            this.ListChannel.Publish(msg);
        }

        /// <summary>
        /// A wrapper to read values from an <see cref="T:ExitGames.Diagnostics.Counter.ICounter"/>.
        /// </summary>
        private class CounterPublisherItem
        {
            /// <summary>
            /// The counter.
            /// </summary>
            private readonly ICounter counter;

            /// <summary>
            /// The counter reader.
            /// </summary>
            private readonly PerformanceCounterReader counterReader;

            /// <summary>
            /// The counter name.
            /// </summary>
            private readonly string name;

            /// <summary>
            /// Try to read the nexxt value.
            /// </summary>
            /// <param name="currentTimeStamp">The current time stamp.</param>
            /// <param name="counterSample">The counter sample.</param>
            /// <returns>True on success.</returns>
            public bool TryGetNextValue(DateTime currentTimeStamp, out CounterSample counterSample)
            {
                float nextValue;
                if (this.counterReader != null)
                {
                    if (this.counterReader.TryGetValue(out nextValue))
                    {
                        counterSample = new CounterSample(currentTimeStamp, nextValue);
                        return true;
                    }
                }
                else
                {
                    nextValue = this.counter.GetNextValue();
                    counterSample = new CounterSample(currentTimeStamp, nextValue);
                    return true;
                }
                counterSample = new CounterSample();
                return false;
            }

            /// <summary>
            /// Initializes a new instance of the <see 
            /// cref="T:ExitGames.Diagnostics.Monitoring.CounterSamplePublisher.CounterPublisherItem"/> class.
            /// </summary>
            /// <param name="counter">The counter.</param>
            /// <param name="name">The counter name.</param>
            public CounterPublisherItem(ICounter counter, string name)
            {
                this.name = name;
                this.counter = counter;
                this.counterReader = counter as PerformanceCounterReader;
            }

            /// <summary>
            /// Gets the counter name.
            /// </summary>
            public string Name
            {
                get
                {
                    return this.name;
                }
            }
        }
    }
}
