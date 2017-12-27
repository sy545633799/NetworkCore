using System;
using System.Collections.Generic;

namespace ExitGames.Concurrency.Core
{
    /// <summary>
    /// Registry for subscriptions. Provides thread safe methods for list of subscriptions.
    /// </summary>
    internal class Subscriptions : IDisposable
    {
        private readonly object _lock = new object();
        private readonly List<IDisposable> _items = new List<IDisposable>();

        /// <summary>
        ///  Add Disposable
        /// </summary>
        /// <param name="disposable1"></param>
        public void Add(IDisposable disposable1)
        {
            lock (_lock)
            {
                _items.Add(disposable1);
            }
        }

        /// <summary>
        /// Disposes all disposables registered in list.
        /// </summary>
        public void Dispose()
        {
            lock (_lock)
            {
                foreach (var victim in _items.ToArray())
                {
                    victim.Dispose();
                }
                _items.Clear();
            }
        }

        /// <summary>
        /// Number of registered disposables.
        /// </summary>
        public int Count
        {
            get
            {
                lock (_lock)
                {
                    return _items.Count;
                }
            }
        }

        /// <summary>
        /// Remove Disposable
        /// </summary>
        /// <param name="toRemove"></param>
        /// <returns></returns>
        public bool Remove(IDisposable toRemove)
        {
            lock (_lock)
            {
                return _items.Remove(toRemove);
            }
        }
    }
}
