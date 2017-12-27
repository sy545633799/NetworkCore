using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace ExitGames.Threading.CachedSingletonFactory
{
    [StructLayout(LayoutKind.Sequential)]
    public struct CacheValue<TKey, TValue>
    {
        public readonly CacheKey<TKey, TValue> Key;
        public readonly TValue Value;
        public CacheValue(CacheKey<TKey, TValue> key, TValue value)
        {
            this.Key = key;
            this.Value = value;
        }

        public override bool Equals(object obj)
        {
            CacheValue<TKey, TValue> value2 = (CacheValue<TKey, TValue>)obj;
            return this.Value.Equals(value2.Value);
        }

        public override int GetHashCode()
        {
            return this.Value.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("CacheValue({0}/{1})", this.Key, this.Value);
        }
    }

}
