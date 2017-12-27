using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace ExitGames.Threading
{
    /// <summary>
    /// This class is a <see cref="T:System.Collections.Generic.Queue`1"/> wrapper that waits at <see 
    /// cref="M:ExitGames.Threading.BlockingQueue`1.Dequeue"/> until an item becomes available.
    /// </summary>
    /// <typeparam name="T">Type of object in queue.</typeparam>
    public class BlockingQueue<T>
    {
        /// <summary>
        /// The default max wait time.
        /// </summary>
        private readonly int defaultTimeout;

        /// <summary>
        /// The max queue length.
        /// </summary>
        private readonly int maxSize;

        /// <summary>
        /// The wrapped queue.
        /// </summary>
        private readonly Queue<T> queue;

        /// <summary>
        /// The sync root.
        /// </summary>
        private readonly object syncRoot;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ExitGames.Threading.BlockingQueue`1"/> class.
        /// </summary>
        /// <param name="maxSize">
        /// The max queue length.
        /// </param>
        /// <param name="lockTimeout">
        /// The max time in milliseconds to wait for a new item.
        /// </param>
        public BlockingQueue(int maxSize, int lockTimeout)
        {
            this.syncRoot = new object();
            this.maxSize = maxSize;
            this.queue = new Queue<T>(maxSize);
            this.defaultTimeout = lockTimeout;
        }

        /// <summary>
        /// Dequeues the next item.
        /// This methods waits until an item becomes available or until the default timeout has expired.
        /// </summary>
        /// <returns>
        /// The next item in queue.
        /// </returns>
        /// <exception cref="T:System.TimeoutException">
        /// No item available within given time.
        /// </exception>
        public T Dequeue()
        {
            return this.Dequeue(this.defaultTimeout);
        }

        /// <summary>
        /// Dequeues the next item.
        /// This methods waits until an item becomes available or until the timeout has expired.
        /// </summary>
        /// <param name="waitTime">
        /// The max wait time in milliseconds.
        /// </param>
        /// <returns>
        /// The next item in queue.
        /// </returns>
        public T Dequeue(int waitTime)
        {
            T local2;
            Stopwatch stopwatch = Stopwatch.StartNew();
            bool flag = true;
            if (!Monitor.TryEnter(this.syncRoot, waitTime))
            {
                throw new TimeoutException();
            }
            try
            {
                while (this.queue.Count == 0)
                {
                    if ((int)(stopwatch.ElapsedMilliseconds) > waitTime)
                    {
                        throw new TimeoutException();
                    }
                    if (!Monitor.Wait(this.syncRoot, waitTime))
                    {
                        flag = false;
                        throw new TimeoutException();
                    }
                }
                stopwatch.Stop();
                local2 = this.queue.Dequeue();
            }
            finally
            {
                if (flag)
                {
                    Monitor.Exit(this.syncRoot);
                }
            }
            return local2;
        }

        /// <summary>
        /// Dequeues the next item.
        /// This methods waits until an item becomes available or until the timeout has expired.
        /// </summary>
        /// <param name="timeout">
        /// The max time to wait.
        /// </param>
        /// <returns>
        /// The next item in queue. 
        /// </returns>
        /// <exception cref="T:System.TimeoutException">
        /// No item available within given time.
        ///</exception>
        public T Dequeue(TimeSpan timeout)
        {
            return this.Dequeue((int)timeout.TotalMilliseconds);
        }

        /// <summary>
        /// Enqueues an item. The default timeout is used to detect dead locks.
        /// </summary>
        /// <param name="item">
        ///  The item. 
        /// </param>
        /// <exception cref="T:System.TimeoutException">
        ///  The item could not be enqueued within the given time.
        /// </exception>
        public void Enqueue(T item)
        {
            this.Enqueue(item, this.defaultTimeout);
        }

        /// <summary>
        /// Enqueues an item. The timeout is used to detect dead locks.
        /// </summary>
        /// <param name="value">
        /// The value. 
        /// </param>
        /// <param name="timeOut">
        /// The time out.
        /// </param>
        /// <exception cref="T:System.TimeoutException">
        /// The item could not be enqueued within the given time.
        ///  </exception>
        public void Enqueue(T value, int timeOut)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            if (!Monitor.TryEnter(this.syncRoot, timeOut))
            {
                throw new TimeoutException("enqueue/dequeue sync");
            }
            try
            {
                stopwatch.Stop();
                this.queue.Enqueue(value);
            }
            finally
            {
                Monitor.Exit(this.syncRoot);
                Monitor.PulseAll(this.syncRoot);
            }
        }

        /// <summary>
        /// Enqueues an item. The timeout is used to detect dead locks.
        /// </summary>
        /// <param name="item">
        /// </param>
        /// The item.
        /// <param name="timeout">
        /// The timeout.
        /// </param>
        /// <exception cref="T:System.TimeoutException">
        /// The item could not be enqueued within the given time.
        /// </exception>
        public void Enqueue(T item, TimeSpan timeout)
        {
            this.Enqueue(item, (int)timeout.TotalMilliseconds);
        }

        /// <summary>
        /// Gets the max queue length.
        /// </summary>
        public int MaxSize
        {
            get
            {
                return this.maxSize;
            }
        }
    }
}
