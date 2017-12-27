using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace ExitGames.Threading
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ReadLock : IDisposable
    {
        private readonly ReaderWriterLockSlim readerWriterLockSlim;
        private ReadLock(ReaderWriterLockSlim slim1)
        {
            this.readerWriterLockSlim = slim1;
            this.readerWriterLockSlim.EnterReadLock();
        }

        private ReadLock(ReaderWriterLockSlim slim1, int millisecondsTimeout)
        {
            this.readerWriterLockSlim = slim1;
            if (!this.readerWriterLockSlim.TryEnterReadLock(millisecondsTimeout))
            {
                throw new LockTimeoutException();
            }
        }

        public static IDisposable Enter(ReaderWriterLockSlim syncObject)
        {
            return new ReadLock(syncObject);
        }

        public static IDisposable TryEnter(ReaderWriterLockSlim syncObject, int millisecondsTimeout)
        {
            return new ReadLock(syncObject, millisecondsTimeout);
        }

        public void Dispose()
        {
            this.readerWriterLockSlim.ExitReadLock();
        }
    }

}
