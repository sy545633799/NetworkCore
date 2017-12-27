using System;
using ExitGames.Concurrency.Fibers;

namespace Photon.SocketServer.Concurrency
{
    /// <summary>
    /// This class is an <see cref="T:ExitGames.Concurrency.Fibers.IFiber"/> wrapper. 
    /// It ensures that async actions are exeucted in a serial manner.
    /// </summary>
    public sealed class ActionQueue
    {
        /// <summary>
        /// The fiber.
        /// </summary>
        private readonly IFiber fiber;

        /// <summary>
        /// The owner.
        /// </summary>
        private readonly object owner;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Photon.SocketServer.Concurrency.ActionQueue"/> class.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <param name="fiber">The fiber.</param>
        public ActionQueue(object owner, IFiber fiber)
        {
            this.owner = owner;
            this.fiber = fiber;
        }

        /// <summary>
        /// Enqueues an action into the <see cref="P:Photon.SocketServer.Concurrency.ActionQueue.Fiber"/>.
        /// </summary>
        /// <param name="action"> The action.</param>
        public void EnqueueAction(Action action)
        {
            this.fiber.Enqueue(action);
        }

        /// <summary>
        /// Schedules an action on the <see cref="P:Photon.SocketServer.Concurrency.ActionQueue.Fiber"/>.
        /// </summary>
        /// <param name="action"> The action.</param>
        /// <param name="timeTilEnqueueInMs"> The time til enqueue in ms.</param>
        /// <returns> a timer control</returns>
        public IDisposable ScheduleAction(Action action, long timeTilEnqueueInMs)
        {
            return this.fiber.Schedule(action, timeTilEnqueueInMs);
        }

        /// <summary>
        /// Schedules an action on an interval.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="timeTilEnqueueInMs"> The time til enqueue in ms.</param>
        /// <returns> a timer control</returns>
        public IDisposable ScheduleActionOnInterval(Action action, long timeTilEnqueueInMs)
        {
            return this.fiber.ScheduleOnInterval(action, timeTilEnqueueInMs, timeTilEnqueueInMs);
        }

        /// <summary>
        /// Gets the underlying <see cref="T:ExitGames.Concurrency.Fibers.IFiber"/>.
        /// </summary>
        public IFiber Fiber
        {
            get
            {
                return this.fiber;
            }
        }

        /// <summary>
        ///  Gets the action queue's owner.
        /// </summary>
        public object Owner
        {
            get
            {
                return this.owner;
            }
        }
    }
}
