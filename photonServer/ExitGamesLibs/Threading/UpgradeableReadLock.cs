using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace ExitGames.Threading
{
    /// <summary>
    /// This <see cref="T:System.IDisposable"/> uses the <see cref="T:System.Threading.ReaderWriterLockSlim"/> for critical sections that allow one writer and multiple reader. 
    /// The counter parts are <see cref="T:ExitGames.Threading.WriteLock"/> and <see cref="T:ExitGames.Threading.ReadLock"/>.
    /// It is used to replaces try-finally blocks with "using" statements.
    /// </summary>
    /// <example>
    ///<code>
    /// using (UpgradeableReadLock.Enter(rwLock))
    ///  {
    /// // critical section here
    ///  }
    ///</code>
    ///</example>
    [StructLayout(LayoutKind.Sequential)]
    public struct UpgradeableReadLock : IDisposable
    {
        /// <summary>
        /// The reader writer lock.
        /// </summary>
        private readonly ReaderWriterLockSlim syncObject;

        /// <summary>
        ///  Initializes a new instance of the <see cref="T:ExitGames.Threading.UpgradeableReadLock"/> struct.
        /// </summary>
        /// <param name="syncObject">The reader writer lock</param>
        private UpgradeableReadLock(ReaderWriterLockSlim syncObject)
        {
            this.syncObject = syncObject;
            this.syncObject.EnterUpgradeableReadLock();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ExitGames.Threading.UpgradeableReadLock"/> struct.
        /// </summary>
        /// <param name="syncObject">
        /// The reader writer lock
        /// </param>
        /// <param name="millisecondsTimeout">
        /// The timeout for <see cref="M:System.Threading.ReaderWriterLockSlim.TryEnterUpgradeableReadLock(System.Int32)"/> in milliseconds.
        /// </param>
        /// <exception cref="T:ExitGames.Threading.LockTimeoutException">
        /// <see cref="M:System.Threading.ReaderWriterLockSlim.TryEnterUpgradeableReadLock(System.Int32)"/> returned false.
        /// </exception>
        private UpgradeableReadLock(ReaderWriterLockSlim syncObject, int millisecondsTimeout)
        {
            this.syncObject = syncObject;
            if (this.syncObject.TryEnterUpgradeableReadLock(millisecondsTimeout))
            {
                throw new LockTimeoutException();
            }
        }

        /// <summary>
        /// Enters a critical section with <see cref="M:System.Threading.ReaderWriterLockSlim.EnterUpgradeableReadLock"/> and returns a new instance of <see cref="T:ExitGames.Threading.UpgradeableReadLock"/>.
        /// </summary>
        /// <param name="syncObject">The reader writer lock.</param>
        /// <returns> 
        /// A <see cref="T:ExitGames.Threading.UpgradeableReadLock"/> that can be disposed to call <see cref="M:System.Threading.ReaderWriterLockSlim.ExitUpgradeableReadLock"/>. 
        ///</returns>
        public static IDisposable Enter(ReaderWriterLockSlim syncObject)
        {
            return new UpgradeableReadLock(syncObject);
        }

        /// <summary>
        /// Enters a critical section with <see cref="M:System.Threading.ReaderWriterLockSlim.TryEnterUpgradeableReadLock(System.Int32)"/> and returns a new instance of <see cref="T:ExitGames.Threading.WriteLock"/>.
        /// </summary>
        /// <param name="syncObject">
        /// The reader writer lock.
        /// </param>
        /// <param name="millisecondsTimeout">
        /// The timeout for <see cref="M:System.Threading.ReaderWriterLockSlim.TryEnterUpgradeableReadLock(System.Int32)"/> in milliseconds.
        /// </param>
        /// <returns>A <see cref="T:ExitGames.Threading.UpgradeableReadLock"/> that can be disposed to call <see cref="M:System.Threading.ReaderWriterLockSlim.ExitUpgradeableReadLock"/>.
        /// </returns>
        /// <exception cref="T:ExitGames.Threading.LockTimeoutException">
        /// <see cref="M:System.Threading.ReaderWriterLockSlim.TryEnterUpgradeableReadLock(System.Int32)"/> returned false.
        ///</exception>
        public static IDisposable TryEnter(ReaderWriterLockSlim syncObject, int millisecondsTimeout)
        {
            return new UpgradeableReadLock(syncObject, millisecondsTimeout);
        }

        /// <summary>
        /// Calls <see cref="M:System.Threading.ReaderWriterLockSlim.ExitUpgradeableReadLock"/>.
        /// </summary>
        public void Dispose()
        {
            this.syncObject.ExitUpgradeableReadLock();
        }
    }
}
