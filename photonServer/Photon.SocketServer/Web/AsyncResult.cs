using System;
using System.Threading;

namespace Photon.SocketServer.Web
{
    internal class AsyncResult : IAsyncResult
    {
        // Fields
        private readonly AsyncCallback asyncCallback;
        private readonly object asyncState;
        private Exception exception;
        private int status;
        private ManualResetEvent waitHandle;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Photon.SocketServer.Web.AsyncResult"/> class.
        /// </summary>
        /// <param name="asyncCallback">The async callback.</param>
        /// <param name="state">The state.</param>
        public AsyncResult(AsyncCallback asyncCallback, object state)
        {
            this.asyncCallback = asyncCallback;
            this.asyncState = state;
        }

        /// <summary>
        /// Private method to create the IAsyncResult AsyncWaitHandle.
        /// </summary>
        /// <remarks>
        /// The WaitHandle, a reference to a ManualResetEvent object, is only needed if 
        /// the code starting the asynchronous operation queries the AsyncWaitHandle 
        /// property or if the code calls the EndInvoke method before the operation 
        /// has actually completed executing.
        /// </remarks>
        private void CreateWaitHandle()
        {
            bool isCompleted = this.IsCompleted;
            ManualResetEvent event2 = new ManualResetEvent(isCompleted);
            if (Interlocked.CompareExchange<ManualResetEvent>(ref this.waitHandle, event2, null) != null)
            {
                event2.Close();
            }
            else if (!isCompleted && this.IsCompleted)
            {
                this.waitHandle.Set();
            }
        }

        /// <summary>
        ///  End invoke.
        /// </summary>
        /// <remarks>This method assumes that only 1 thread calls EndInvoke for this object</remarks>
        public void EndInvoke()
        {
            if (!this.IsCompleted)
            {
                this.AsyncWaitHandle.WaitOne();
                this.AsyncWaitHandle.Close();
                this.waitHandle = null;
            }
            if (this.exception != null)
            {
                throw this.exception;
            }
        }

        public void SetCompleted(Exception completedException, bool completedSynchronously)
        {
            this.exception = completedException;
            int num = completedSynchronously ? 1 : 2;
            if (Interlocked.CompareExchange(ref this.status, num, 0) != 0)
            {
                throw new InvalidOperationException("SetCompleted can only called once.");
            }
            if (this.waitHandle != null)
            {
                this.waitHandle.Set();
            }
            if (this.asyncCallback != null)
            {
                this.asyncCallback(this);
            }
        }

        /// <summary>
        /// Gets a user-defined object that qualifies or contains 
        /// information about an asynchronous operation.
        /// </summary>
        public object AsyncState
        {
            get
            {
                return this.asyncState;
            }
        }

        /// <summary>
        /// Gets a <see cref="T:System.Threading.WaitHandle"/> that is used 
        /// to wait for an asynchronous operation to complete.
        /// </summary>
        public WaitHandle AsyncWaitHandle
        {
            get
            {
                if (this.waitHandle == null)
                {
                    this.CreateWaitHandle();
                }
                return this.waitHandle;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the asynchronous 
        ///   operation completed synchronously.
        /// </summary>
        public bool CompletedSynchronously
        {
            get
            {
                return (this.Status == AsyncResultState.CompletedSynchronously);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the asynchronous 
        /// operation has completed. 
        /// </summary>
        public bool IsCompleted
        {
            get
            {
                return (this.Status != AsyncResultState.Pending);
            }
        }

        /// <summary>
        /// Gets the current status.
        /// </summary>
        public AsyncResultState Status
        {
            get
            {
                return (AsyncResultState)Thread.VolatileRead(ref this.status);
            }
        }

        // Nested Types
        public enum AsyncResultState
        {
            Pending,
            CompletedSynchronously,
            CompletedAsynchronously
        }
    }
}
