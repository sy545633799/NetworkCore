using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExitGames.Threading
{
    /// <summary>
    /// </summary>
    /// <typeparam name="TKey">The type of key. </typeparam>
    /// <typeparam name="TValue">The type of value. </typeparam>
    /// <param name="key">The key.</param>
    /// <param name="value">The output value.</param>
    /// <returns>True if a value was created. Otherwise false. </returns>
    public delegate bool CreateMethodDelegate<TKey, TValue>(TKey key, out TValue value);

}
