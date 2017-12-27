using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace ExitGames.Threading
{
    /// <summary>
    ///  This IDisposable uses the Monitor class. It is used to replaces try-finally blocks with "using" statements. 
    /// </summary>
    /// <example>
    /// <code>
    ///  using (ReadLock.Enter(rwLock))
    ///  {
    ///  // critical section here
    ///  }
    /// </code>
    /// </example>
    [StructLayout(LayoutKind.Sequential)]
    public struct Lock : IDisposable
    {
        private readonly object lockObject;
        private Lock(object obj)
        {
            this.lockObject = obj;
            Monitor.TryEnter(obj);
        }

        private Lock(object obj, int timeout)
        {
            this.lockObject = obj;
            if (!Monitor.TryEnter(obj, timeout))
            {              
                throw new LockTimeoutException();
            }
        }

        public static IDisposable Enter(object syncObject)
        {
            return new Lock(syncObject);
        }

        public static IDisposable TryEnter(object syncObject, int millisecondsTimeout)
        {
            return new Lock(syncObject, millisecondsTimeout);
        }

        public void Dispose()
        {
            Monitor.Exit(this.lockObject);
        }
    }

}
