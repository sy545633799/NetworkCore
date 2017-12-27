using System;
using System.Collections.Generic;
using System.Threading;

namespace ExitGames.Threading
{
    /// <summary>
    /// Represents a thread-safe collection of key-value pairs that can be accessed by multiple threads concurrently. If using the Net 4.0 framework you should consider to use the new System.Collections.Concurrent.ConcurrentDictionary. The SynchronizedDictionary provides methods similar to the ConcurrentDictionary to provide a thread safe Dictionary for .NET 3.5 and earlier. 
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    public class SynchronizedDictionary<TKey, TValue> : IDisposable
    {
        /// <summary>
        /// The wrapped dictionary.
        /// </summary>
        private readonly Dictionary<TKey, TValue> dictionary;

        /// <summary>
        /// The locking mechanism.
        /// </summary>
        private readonly ReaderWriterLockSlim readerWriterLock;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ExitGames.Threading.SynchronizedDictionary`2"/> class. 
        /// </summary>
        public SynchronizedDictionary()
        {
            dictionary = new Dictionary<TKey, TValue>();
            readerWriterLock = new ReaderWriterLockSlim();
            dictionary = new Dictionary<TKey, TValue>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ExitGames.Threading.SynchronizedDictionary`2"/> class. 
        /// </summary>
        /// <param name="capacity"> The <see cref="T:System.Collections.Generic.IDictionary`2"/> whose elements are copied to 
        ///  the new SynchronizedDictionary.</param>
        public SynchronizedDictionary(IDictionary<TKey, TValue> dictionary)
        {
            dictionary = new Dictionary<TKey, TValue>();
            readerWriterLock = new ReaderWriterLockSlim();
            dictionary = new Dictionary<TKey, TValue>(dictionary);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ExitGames.Threading.SynchronizedDictionary`2"/> class. 
        /// </summary>
        /// <param name="comparer">The <see cref="T:System.Collections.Generic.IEqualityComparer`1"/> implementation to use 
        ///    when comparing keys.</param>
        public SynchronizedDictionary(IEqualityComparer<TKey> comparer)
        {
            dictionary = new Dictionary<TKey, TValue>();
            readerWriterLock = new ReaderWriterLockSlim();
            dictionary = new Dictionary<TKey, TValue>(comparer);
        }

        /// <summary>
        ///  Initializes a new instance of the <see cref="T:ExitGames.Threading.SynchronizedDictionary`2"/> class.
        /// </summary>
        /// <param name="capacity">The initial number of elements that the SynchronizedDictionary can contain.</param>
        public SynchronizedDictionary(int capacity)
        {
            dictionary = new Dictionary<TKey, TValue>();
            readerWriterLock = new ReaderWriterLockSlim();
            dictionary = new Dictionary<TKey, TValue>(capacity);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ExitGames.Threading.SynchronizedDictionary`2"/> class.
        /// </summary>
        /// <param name="dictionary">
        /// <param name="dictionary">
        ///  The <see cref="T:System.Collections.Generic.IDictionary`2"/> whose elements are copied to 
        ///   the new SynchronizedDictionary.
        /// </param>
        /// <param name="comparer">
        /// The <see cref="T:System.Collections.Generic.IEqualityComparer`1"/> implementation to use
        ///   when comparing keys.
        /// </param>
        public SynchronizedDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
        {
            dictionary = new Dictionary<TKey, TValue>();
            readerWriterLock = new ReaderWriterLockSlim();
            dictionary = new Dictionary<TKey, TValue>(dictionary, comparer);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ExitGames.Threading.SynchronizedDictionary`2"/> class. 
        /// </summary>
        /// <param name="capacity">
        /// The initial number of elements that the SynchronizedDictionary can contain.
        /// </param>
        /// <param name="comparer">
        /// The <see cref="T:System.Collections.Generic.IEqualityComparer`1"/> implementation to use
        ///  when comparing keys.
        ///  </param>
        public SynchronizedDictionary(int capacity, IEqualityComparer<TKey> comparer)
        {
            dictionary = new Dictionary<TKey, TValue>();
            readerWriterLock = new ReaderWriterLockSlim();
            dictionary = new Dictionary<TKey, TValue>(capacity, comparer);
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="T:ExitGames.Threading.SynchronizedDictionary`2"/> class.
        /// </summary>
        ~SynchronizedDictionary()
        {
            Dispose(false);
        }

        /// <summary>
        /// Adds a key/value pair to the dictionary if the key does not already exist, 
        ///   or updates a key/value pair in the dictionary if the key already exists.
        /// </summary>
        /// <param name="key">The key to be added or whose value should be updated.</param>
        /// <param name="addValue">The value to be added for an absent key.</param>
        /// <param name="updateValueFactory">The function used to generate a new value for an existing key based on the key's existing value.</param>
        /// <returns>The new value for the key. This will be either be addValue (if the key was absent) or the result of updateValueFactory (if the key was present).</returns>
        public TValue AddOrUpdate(TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory)
        {
            using (WriteLock.Enter(readerWriterLock))
            {
                TValue local;
                if (dictionary.TryGetValue(key, out local))
                {
                    TValue local2 = updateValueFactory(key, local);
                    dictionary[key] = local2;
                    return local2;
                }
                dictionary.Add(key, addValue);
            }
            return addValue;
        }

        /// <summary>
        /// Adds a key/value pair to the dictionary if the key does not already exist, 
        ///  or updates a key/value pair in the dictionary if the key already exists.
        /// </summary>
        /// <param name="key">The key to be added or whose value should be updated.</param>
        /// <param name="addValueFactory">The function used to generate a value for an absent key.</param>
        /// <param name="updateValueFactory">The function used to generate a new value for an existing key based on the key's existing value.</param>
        /// <returns>The new value for the key. This will be either be the result of addValueFactory (if the key was absent) or the result of updateValueFactory (if the key was present).</returns>
        public TValue AddOrUpdate(TKey key, Func<TKey, TValue> addValueFactory, Func<TKey, TValue, TValue> updateValueFactory)
        {
            TValue local3;
            using (WriteLock.Enter(readerWriterLock))
            {
                TValue local;
                TValue local2 = dictionary.TryGetValue(key, out local) ? updateValueFactory(key, local) : addValueFactory(key);
                dictionary[key] = local2;
                local3 = local2;
            }
            return local3;
        }

        /// <summary>
        /// Removes all keys and values from the <see cref="T:ExitGames.Threading.SynchronizedDictionary`2"/>.
        /// </summary>
        public void Clear()
        {
            using (WriteLock.Enter(readerWriterLock))
            {
                dictionary.Clear();
            }
        }

        /// <summary>
        /// Determines whether the <see cref="T:ExitGames.Threading.SynchronizedDictionary`2"/> contains the specified key.
        /// </summary>
        /// <param name="key">The key to locate.</param>
        /// <returns>True if the <see cref="T:ExitGames.Threading.SynchronizedDictionary`2"/> contains an element with the specified key;
        ///otherwise, false.</returns>
        public bool ContainsKey(TKey key)
        {
            bool flag;
            using (ReadLock.Enter(readerWriterLock))
            {
                flag = dictionary.ContainsKey(key);
            }
            return flag;
        }

        /// <summary>
        /// Adds a key/value pair to the <see cref="T:ExitGames.Threading.SynchronizedDictionary`2"/> if the key 
        /// does not already exist
        /// </summary>
        /// <param name="key">The key of the element to get or add.</param>
        /// <param name="valueFactory">The function used to generate a value for the key</param>
        /// <returns>The value for the key. This will be either the existing value for the key if the
        /// key is already in the dictionary, or the new value for the key as returned by valueFactory 
        /// if the key was not in the dictionary.</returns>
        public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
        {
            TValue local2;
            using( UpgradeableReadLock.Enter(readerWriterLock))
            {
                TValue local;
                if (dictionary.TryGetValue(key, out local))
                {
                    local2 = local;
                }
                else
                {
                    using(WriteLock.Enter(readerWriterLock))
                    {
                        local = valueFactory(key);
                        dictionary.Add(key, local);
                        local2 = local;
                    }
                }
            }
            return local2;
        }

        /// <summary>
        /// Removes the value with the specified key from the <see cref="T:ExitGames.Threading.SynchronizedDictionary`2"/>.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <returns>true if the element is successfully found and removed; otherwise, false. 
        /// This method returns false if key is not found in the <see cref="T:ExitGames.Threading.SynchronizedDictionary`2"/>.</returns>
        public bool Remove(TKey key)
        {
            bool flag;
            using (WriteLock.Enter(readerWriterLock))
            {
                flag = dictionary.Remove(key);
            }
            return flag;
        }

        /// <summary>
        /// Attempts to add the specified key and value to the <see cref="T:ExitGames.Threading.SynchronizedDictionary`2"/>.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value of the element to add. The value can be a null reference (Nothing in Visual Basic) for reference types.</param>
        /// <returns>True if the key/value pair was added to the <see cref="T:ExitGames.Threading.SynchronizedDictionary`2"/> successfully. 
        ///  If the key already exists, this method returns false.</returns>
        public bool TryAdd(TKey key, TValue value)
        {
            bool flag;
            using (WriteLock.Enter(readerWriterLock))
            {
                if (dictionary.ContainsKey(key))
                {
                    return false;
                }
                dictionary.Add(key, value);
                flag = true;
            }
            return flag;
        }

        /// <summary>
        /// Attempts to get the value associated with the specified key from the <see cref="T:ExitGames.Threading.SynchronizedDictionary`2"/>.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="value">When this method returns true, value contains the object from the <see cref="T:ExitGames.Threading.SynchronizedDictionary`2"/>
        ///  with the specified key.</param>
        /// <returns>true if the key was found in the <see cref="T:ExitGames.Threading.SynchronizedDictionary`2"/>; otherwise, false.</returns>
        public bool TryGetValue(TKey key, out TValue value)
        {
            bool flag;
            using (ReadLock.Enter(readerWriterLock))
            {
                flag = dictionary.TryGetValue(key, out value);
            }
            return flag;
        }

        /// <summary>
        /// Attempts to remove and return the value with the specified key from the <see cref="T:ExitGames.Threading.SynchronizedDictionary`2"/>.
        /// </summary>
        /// <param name="key">The key of the element to remove and return.</param>
        /// <param name="value">When this method returns true, value contains the object removed from the <see cref="T:ExitGames.Threading.SynchronizedDictionary`2"/> or the default value of if the operation failed.</param>
        /// <returns>true if an object was removed successfully; otherwise, false.</returns>
        public bool TryRemove(TKey key, out TValue value)
        {
            bool flag;
            using (WriteLock.Enter(readerWriterLock))
            {
                if (dictionary.TryGetValue(key, out value))
                {
                    dictionary.Remove(key);
                    return true;
                }
                flag = false;
            }
            return flag;
        }

        /// <summary>
        /// Compares the existing value for the specified key with a specified value, 
        /// and if they are equal, updates the key with a third value.
        /// </summary>
        /// <param name="key">The key whose value is compared with comparisonValue and possibly replaced.</param>
        /// <param name="newValue">The value that replaces the value of the element with key if the comparison results in equality</param>
        /// <param name="comparisonValue">The value that is compared to the value of the element with key</param>
        /// <returns>true if the value with key was equal to comparisonValue and replaced with newValue; otherwise, false.</returns>
        public bool TryUpdate(TKey key, TValue newValue, TValue comparisonValue)
        {
            bool flag;
            using (UpgradeableReadLock.Enter(readerWriterLock))
            {
                TValue local;
                if (!dictionary.TryGetValue(key, out local))
                {
                    return false;
                }
                if (!local.Equals(comparisonValue))
                {
                    flag = false;
                }
                else
                {
                    using (WriteLock.Enter(readerWriterLock))
                    {
                        dictionary[key] = newValue;
                        flag = true;
                    }
                }
            }
            return flag;
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="T:ExitGames.Threading.SynchronizedDictionary`2"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing">
        /// <c>true</c> to release both managed and unmanaged resources; 
        /// <c>false</c> to release only unmanaged resources.
        /// </param>
        /// <remarks>
        /// Dispose(bool disposing) executes in two distinct scenarios.
        /// If disposing equals true, the method has been called directly
        /// or indirectly by a user's code. Managed and unmanaged resources
        ///can be disposed.
        ///If disposing equals false, the method has been called by the
        ///runtime from inside the finalizer and you should not reference
        ///other objects. Only unmanaged resources can be disposed.
        ///</remarks>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                readerWriterLock.Dispose();
            }
        }

        /// <summary>
        /// Gets the number of key/value pairs contained in the <see cref="T:ExitGames.Threading.SynchronizedDictionary`2"/>.
        /// </summary>
        /// <value>The number of key/value pairs contained in the <see cref="T:ExitGames.Threading.SynchronizedDictionary`2"/></value>
        public int Count
        {
            get
            {
                int count;
                using (ReadLock.Enter(readerWriterLock))
                {
                    count = dictionary.Count;
                }
                return count;
            }
        }

        /// <summary>
        /// Gets or sets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get or set.</param>
        /// <returns>The value for the key.</returns>
        public TValue this[TKey key]
        {
            get
            {
                TValue local;
                using (ReadLock.Enter(readerWriterLock))
                {
                    local = dictionary[key];
                }
                return local;
            }
            set
            {
                using (WriteLock.Enter(readerWriterLock))
                {
                    dictionary[key] = value;
                }
            }
        }
    }

}
