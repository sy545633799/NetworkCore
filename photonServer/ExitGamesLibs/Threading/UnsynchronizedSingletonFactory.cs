using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ExitGames.Threading
{
    /// <summary>
    /// This class is used to create instances that are unique per key. 
    /// </summary>
    /// <typeparam name="TKey">The type of key.</typeparam>
    /// <typeparam name="TValue">The type of value.</typeparam>
    /// <remarks>
    ///  Instance members are not thread safe.
    ///  For multi-threaded environments use the <see cref="T:ExitGames.Threading.SynchronizedSingletonFactory`2"/>.
    /// </remarks>
    [DebuggerStepThrough]
    public class UnsynchronizedSingletonFactory<TKey, TValue>
    {
        /// <summary>
        /// A dictionary for all instances.
        /// </summary>
        private readonly Dictionary<TKey, TValue> instances;

        /// <summary>
        /// The delegate that creates new instances.
        /// </summary>
        private CreateMethodDelegate<TKey, TValue> defaultCreateMethod;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ExitGames.Threading.UnsynchronizedSingletonFactory`2"/> class.
        /// </summary>
        /// <param name="defaultCreateMethod">
        /// The default function that creates new instances. 
        /// </param>
        public UnsynchronizedSingletonFactory(CreateMethodDelegate<TKey, TValue> defaultCreateMethod)
        {
            this.defaultCreateMethod = defaultCreateMethod;
            this.instances = new Dictionary<TKey, TValue>();
        }

        /// <summary>
        /// Adds a new value if there is none for the same key.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <returns>
        /// True if the <paramref name = "value" /> has been added. False if another value for this <paramref name = "key" /> has already been present.
        /// </returns>
        public virtual bool Add(TKey key, TValue value)
        {
            if (!this.Instances.ContainsKey(key))
            {
                this.DoAdd(key, value);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Adds a value to the <see cref="P:ExitGames.Threading.UnsynchronizedSingletonFactory`2.Instances"/>.
        /// This method is called from <see cref="M:ExitGames.Threading.UnsynchronizedSingletonFactory`2.Add(`0,`1)"/> and <see
        /// cref="M:ExitGames.Threading.UnsynchronizedSingletonFactory`2.Get(`0,ExitGames.Threading.CreateMethodDelegate{`0,`1})">Get</see>.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        protected virtual void DoAdd(TKey key, TValue value)
        {
            this.Instances.Add(key, value);
        }

        /// <summary>
        /// This method iterates over a copy of <see cref="P:ExitGames.Threading.UnsynchronizedSingletonFactory`2.Instances"/> and executes the <paramref name="action"/> on each item.
        /// </summary>
        /// <param name="action">
        /// The action.
        /// </param>
        public void ForAll(Action<TValue> action)
        {
            Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>(this.Instances);
            foreach (TValue local in dictionary.Values)
            {
                action(local);
            }
        }

        /// <summary>
        /// This method iterates over a copy of <see cref="P:ExitGames.Threading.UnsynchronizedSingletonFactory`2.Instances"/>. 
        /// The <paramref name="selector"/> parameter selects a value of each instance.
        /// These values are combined with the <paramref name="aggregateFunction"/>.
        /// </summary>
        /// <typeparam name="TResult">
        /// The action that maps a value to each instance.
        /// </typeparam>
        /// <param name="selector">
        /// The function that combines all <paramref name="selector"/> results.
        /// </param>
        /// <param name="aggregateFunction">
        /// The result value to start with.
        /// </param>
        /// <param name="seed">
        /// The type of the result value.
        /// </param>
        /// <returns>
        /// An aggregegated value from all instances.
        /// </returns>
        public TResult ForAll<TResult>(Func<TValue, TResult> selector, Func<TResult, TResult, TResult> aggregateFunction, TResult seed)
        {
            Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>(this.Instances);
            return Enumerable.Aggregate<TResult, TResult>(Enumerable.Select<TValue, TResult>(dictionary.Values, selector), seed, aggregateFunction);
        }

        /// <summary>
        /// Gets an existing value for a key or creates a new one.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <returns>
        /// The value for the key.
        /// </returns>
        public TValue Get(TKey key)
        {
            return this.Get(key, this.defaultCreateMethod);
        }

        /// <summary>
        /// Gets an existing value for a key or creates a new one with the default <see cref="P:ExitGames.Threading.UnsynchronizedSingletonFactory`2.CreateMethod"/>.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="createMethod">
        /// The method that creates a new value if the key has not been added yet.
        /// </param>
        /// <returns>
        /// The value for the key.
        /// </returns>
        public virtual TValue Get(TKey key, CreateMethodDelegate<TKey, TValue> createMethod)
        {
            TValue local;
            TValue local2;
            if (this.TryGet(key, out local))
            {
                return local;
            }
            if (createMethod(key, out local2))
            {
                local = local2;
                this.DoAdd(key, local);
                return local;
            }
            return default(TValue);
        }

        /// <summary>
        ///  Removes a value from the <see cref="P:ExitGames.Threading.UnsynchronizedSingletonFactory`2.Instances"/>.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <returns>
        /// True if key was found and removed, otherwise false.
        /// </returns>
        public virtual bool Remove(TKey key)
        {
            return this.Instances.Remove(key);
        }

        /// <summary>
        /// Tries to get an existing value for the key.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <returns>
        /// True if a value was found, otherwise false.
        /// </returns>
        public virtual bool TryGet(TKey key, out TValue value)
        {
            return this.Instances.TryGetValue(key, out value);
        }

        /// <summary>
        /// Gets the number of existing instances.
        /// </summary>
        public int Count
        {
            get
            {
                return this.Instances.Count;
            }
        }

        /// <summary>
        /// Gets or sets the default function that creates new instances. 
        /// </summary>
        public CreateMethodDelegate<TKey, TValue> CreateMethod
        {
            get
            {
                return this.defaultCreateMethod;
            }
            set
            {
                this.defaultCreateMethod = value;
            }
        }

        /// <summary>
        /// Gets a reference to the underlying dictionary that contains all existing instances.
        /// </summary>
        protected Dictionary<TKey, TValue> Instances
        {
            get
            {
                return this.instances;
            }
        }
    }

}
