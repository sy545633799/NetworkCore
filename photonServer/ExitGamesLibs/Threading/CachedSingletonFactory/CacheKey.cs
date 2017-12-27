using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace ExitGames.Threading.CachedSingletonFactory
{
    [StructLayout(LayoutKind.Sequential)]
    public struct CacheKey<TKey, TValue>
    {
        public readonly string AspCacheKey;
        public readonly CreateMethodDelegate<TKey, TValue> CreateMethod;
        public readonly TKey Key;
        public CacheKey(string cacheKey, TKey key, CreateMethodDelegate<TKey, TValue> createMethod)
        {
            this.Key = key;
            this.AspCacheKey = cacheKey;
            this.CreateMethod = createMethod;
        }

        public override bool Equals(object obj)
        {
            CacheKey<TKey, TValue> key = (CacheKey<TKey, TValue>)obj;
            return this.Key.Equals(key.Key);
        }

        public override int GetHashCode()
        {
            return this.Key.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("CacheKey({0}/{1})", this.AspCacheKey, this.Key);
        }
    }

}
