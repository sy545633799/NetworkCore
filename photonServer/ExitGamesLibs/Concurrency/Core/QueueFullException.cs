using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExitGames.Concurrency.Core
{
    /// <summary>
    /// Thrown when a queue is full.
    /// </summary>
    public class QueueFullException : Exception
    {
        /// <summary>
        /// Construct the execution with the depth of the queue.
        /// </summary>
        private readonly int depth;

        /// <summary>
        /// Construct with a custom message.
        /// </summary>
        /// <param name="depth"></param>
        public QueueFullException(int depth)
            : base("Attempted to enqueue item into full queue:" + depth)
        { }

        /// <summary>
        /// Construct with a custom message.
        /// </summary>
        /// <param name="msg"></param>
        public QueueFullException(string msg)
            : base(msg)
        { }

        /// <summary>
        ///  Depth of queue.
        /// </summary>
        public int Depth
        {
            get
            {
                return this.depth;
            }
        }
    }

}
