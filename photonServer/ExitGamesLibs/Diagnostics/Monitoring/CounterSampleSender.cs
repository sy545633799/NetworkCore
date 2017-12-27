using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using ExitGames.Concurrency.Channels;
using ExitGames.Concurrency.Fibers;
using ExitGames.Logging;
using ExitGames.Net.Sockets;

namespace ExitGames.Diagnostics.Monitoring
{
    /// <summary>
    /// Collects data from <see cref="T:ExitGames.Diagnostics.Counter.ICounter"/> instances and publishes the data in a specified 
    /// interval using an <see cref="T:ExitGames.Net.Sockets.ISocketSender"/> implementation.
    /// </summary>
    public class CounterSampleSender : IDisposable
    {
        /// <summary>
        /// Gets a value indicating how many packages should be enqueued for republishing if an error opccured during publishing.
        /// The default value is 120.
        /// </summary>
        public readonly int MaxQueueLength = 120;

        /// <summary>
        /// Gets a value indicating how many error can occure during pubishing before the counter publishers stops publishing.
        ///  If a value equal or less than than zero is specified the counter publisher will never stop if an error occured.
        ///  The default value is -1;
        /// </summary>
        public readonly int MaxRetryCount = -1;

        /// <summary>
        /// Provides a <see cref="T:ExitGames.Logging.ILogger"/> instance used to log messages into the logging framework.
        /// </summary>
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        ///  The fiber for async sending.
        /// </summary>
        private readonly PoolFiber fiber = new PoolFiber();

        /// <summary>
        /// The packages to combine.
        /// </summary>
        private readonly int maxCounterPerPackage = 25;

        /// <summary>
        ///  The publish interval in seconds.
        /// </summary>
        private readonly int publishInterval = 10;

        /// <summary>
        /// A dictionary that stores a <see cref="T:ExitGames.Diagnostics.Monitoring.CounterSampleCollection"/> per counter name.
        /// </summary>
        private readonly Dictionary<string, CounterSampleCollection> sampleDictionary = new Dictionary<string, CounterSampleCollection>();

        /// <summary>
        ///  The local sender id.
        /// </summary>
        private readonly string senderId;

        /// <summary>
        /// The used <see cref="T:ExitGames.Net.Sockets.ISocketSender"/>.
        /// </summary>
        private readonly ISocketSender socketSender;

        /// <summary>
        ///  The sync root.
        /// </summary>
        private readonly object syncRoot = new object();

        /// <summary>
        /// The current sample count.
        /// </summary>
        private int sampleCount;

        /// <summary>
        ///  0 = stopped, 1 = running.
        /// </summary>
        private long state;

        private readonly LinkedList<byte[]> queue;

        /// <summary>
        /// The current timer.
        /// </summary>
        private IDisposable timerControl;

        /// <summary>
        ///  Initializes a new instance of the <see cref="T:ExitGames.Diagnostics.Monitoring.CounterSampleSender"/> class.
        /// </summary>
        /// <param name="senderId"> The sender id.</param>
        /// <param name="publishInterval">The publish interval.</param>
        /// <param name="socketSender"> The socket sender.</param>
        public CounterSampleSender(string senderId, int publishInterval, ISocketSender socketSender)
        {
            this.queue = new LinkedList<byte[]>();
            this.senderId = senderId;
            this.publishInterval = publishInterval;
            this.socketSender = socketSender;
        }

        public CounterSampleSender(string senderId, int publishInterval, ISocketSender socketSender, int maxCounterPerPackage)
            : this(senderId, publishInterval, socketSender)
        {
            this.maxCounterPerPackage = maxCounterPerPackage;
        }

        public CounterSampleSender(string senderId, int publishInterval, ISocketSender socketSender, int maxItemsInQueue, int maxSendRetries, int maxCounterPerPackage)
        {
            this.queue = new LinkedList<byte[]>();
            this.senderId = senderId;
            this.publishInterval = publishInterval;
            this.socketSender = socketSender;
            this.MaxQueueLength = maxItemsInQueue;
            this.MaxRetryCount = maxSendRetries;
            this.maxCounterPerPackage = maxCounterPerPackage;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="T:ExitGames.Diagnostics.Monitoring.CounterSampleSender"/> class.
        /// </summary>
        ~CounterSampleSender()
        {
            Dispose(false);
        }

        /// <summary>
        ///  Starts this instance to collect and publish counter data.
        /// </summary>
        public void Start()
        {
            lock (syncRoot)
            {
                if (timerControl == null)
                {
                    Interlocked.Exchange(ref state, 1L);
                    fiber.Start();
                    timerControl = fiber.ScheduleOnInterval(Publish, MaxRetryCount * 1000, MaxRetryCount * 1000);
                }
            }
        }

        /// <summary>
        /// Stops this instance.
        /// </summary>
        public void Stop()
        {
            lock (syncRoot)
            {
                Interlocked.Exchange(ref state, 0L);
                fiber.Stop();
                if (timerControl != null)
                {
                    timerControl.Dispose();
                    timerControl = null;
                }
                sampleDictionary.Clear();
            }
        }

        /// <summary>
        /// Subscribes the sender to a channel of <see cref="T:ExitGames.Diagnostics.Monitoring.CounterSampleMessage"/>s.
        /// </summary>
        /// <param name="channel"> The channel.</param>
        /// <returns> A subscription.</returns>
        public IDisposable SubscribeToChannel(Channel<CounterSampleMessage> channel)
        {
            return channel.Subscribe(fiber, OnCounterSample);
        }

        /// <summary>
        ///  Disposes this instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Stops the sender.
        /// </summary>
        /// <param name="disposing">The disposing.</param>
        protected virtual void Dispose(bool disposing)
        {
            lock (syncRoot)
            {
                if (disposing)
                {
                    Stop();
                }
            }
        }
        /// <summary>
        /// Callback for channel subscriptions.
        /// </summary>
        /// <param name="counterSampleMessage">The counter sample message.</param>
        private void OnCounterSample(CounterSampleMessage counterSampleMessage)
        {
            if (Interlocked.Read(ref state) == 1L)
            {
                CounterSampleCollection samples;
                if (!sampleDictionary.TryGetValue(counterSampleMessage.CounterName, out samples))
                {
                    samples = new CounterSampleCollection(counterSampleMessage.CounterName);
                    sampleDictionary.Add(counterSampleMessage.CounterName, samples);
                }
                samples.Add(counterSampleMessage.CounterSample);
                sampleCount++;
            }
        }

        /// <summary>
        /// Sends samples to the socket sender.
        /// </summary>
        private void Publish()
        {
            if (!socketSender.Connected)
            {
                RaiseOnDisconnetedEvent();
            }
            if (sampleCount > 0)
            {
                if (log.IsDebugEnabled)
                {
                    log.DebugFormat("Publishing counter data.", new object[0]);
                }
                CounterSampleCollection[] array = new CounterSampleCollection[sampleDictionary.Count];
                sampleDictionary.Values.CopyTo(array, 0);
                sampleDictionary.Clear();
                using (MemoryStream output = new MemoryStream())
                {
                    using (BinaryWriter binaryWriter = new BinaryWriter(output))
                    {
                        for (int i = 0; i < array.Length; i += MaxQueueLength)
                        {
                            int num2 = array.Length - i;
                            if (num2 > MaxQueueLength)
                            {
                                num2 = MaxQueueLength;
                            }
                            binaryWriter.Write(0xffee);
                            binaryWriter.Write(DateTime.UtcNow.ToBinary());
                            binaryWriter.Write(num2);
                            binaryWriter.Write(senderId);
                            for (int j = i; j < (i + num2); j++)
                            {
                                array[j].Serialize(binaryWriter);
                            }
                            byte[] buffer = output.ToArray();
                            Add(buffer);
                            output.Position = 0;
                        }
                    }
                }
                fiber.Enqueue(Send);
            }
        }

        /// <summary>
        /// Invokes the <see cref="E:ExitGames.Diagnostics.Monitoring.CounterSampleSender.OnError"/> event.
        /// </summary>
        /// <param name="exception">The exception.</param>
        private void RaiseErrorEvent(Exception exception)
        {
            if (OnError != null)
            {
                OnError(this, new UnhandledExceptionEventArgs(exception, false));
            }
        }

        /// <summary>
        /// Invokes the <see cref="E:ExitGames.Diagnostics.Monitoring.CounterSampleSender.OnDisconnected"/> event.
        /// </summary>
        private void RaiseOnDisconnetedEvent()
        {
            if (OnDisconnected != null)
            {
                OnDisconnected(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Occurs when the underling <see cref="T:ExitGames.Net.Sockets.ISocketSender"/> is diconnected.
        /// </summary>
        public event EventHandler OnDisconnected;

        /// <summary>
        /// Fired when an unhandled Exception occurs during publishing.
        ///  The instance stops publishing data if that happens.
        /// </summary>
        public event EventHandler<UnhandledExceptionEventArgs> OnError;

        private void Add(byte[] buffer)
        {
            queue.AddLast(buffer);
            if (queue.Count > MaxQueueLength)
            {
                queue.RemoveFirst();
            }
        }

        private void Send()
        {
            if (!socketSender.Connected)
            {
                RaiseOnDisconnetedEvent();
            }
            else if (queue.Count != 0)
            {
                try
                {
                    LinkedListNode<byte[]> first = queue.First;
                    socketSender.Send(first.Value);
                    ErrorCount = 0;
                    queue.RemoveFirst();
                    if (queue.Count > 0)
                    {
                        fiber.Enqueue(Send);
                    }
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
                    ErrorCount++;
                    RaiseErrorEvent(exception);
                    if (log.IsDebugEnabled)
                    {
                        log.DebugFormat("Publishing counter data failed for the {0} time: {1}", ErrorCount, exception);
                    }
                    if (MaxRetryCount > 0 && ErrorCount >= MaxRetryCount)
                    {
                        Stop();
                        if (log.IsDebugEnabled)
                        {
                            log.DebugFormat("Maximum errors for publishing counter reached. Counter publisher has beeen stopped.", new object[0]);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the underling <see cref="T:ExitGames.Net.Sockets.ISocketSender"/> is connected 
        /// to a remote host as of the last Send operation.
        /// </summary>
        public bool Connected
        {
            get
            {
                return socketSender.Connected;
            }
        }

        /// <summary>
        ///  Gets the number of errors that occured since the last succesfull publish.
        /// The counter publisher tries to publish the data again until the <see cref="F:ExitGames.Diagnostics.Monitoring.CounterSampleSender.MaxRetryCount"/> is reached.
        /// </summary>
        public int ErrorCount
        {
            get;
            private set;
        }
    }
}
