using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace ExitGames.Threading
{
    [StructLayout(LayoutKind.Sequential)]
    public struct WriteLock : IDisposable
    {
        private readonly ReaderWriterLockSlim readerWriterLockSlim;
        private WriteLock(ReaderWriterLockSlim slim1)
        {
            this.readerWriterLockSlim = slim1;
            this.readerWriterLockSlim.EnterWriteLock();
        }

        private WriteLock(ReaderWriterLockSlim slim1, int millisecondsTimeout)
        {
            this.readerWriterLockSlim = slim1;
            if (!this.readerWriterLockSlim.TryEnterWriteLock(millisecondsTimeout))
            {
                throw new LockTimeoutException();
            }
        }

        public static IDisposable Enter(ReaderWriterLockSlim syncObject)
        {
            return new WriteLock(syncObject);
        }

        public static IDisposable TryEnter(ReaderWriterLockSlim syncObject, int millisecondsTimeout)
        {
            return new WriteLock(syncObject, millisecondsTimeout);
        }

        public void Dispose()
        {
            this.readerWriterLockSlim.EnterWriteLock();
        }
    }

}
