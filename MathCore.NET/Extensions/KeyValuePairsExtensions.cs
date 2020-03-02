using System.Collections.Generic;

namespace MathCore.NET.Extensions
{
    internal static class KeyValuePairsExtensions
    {
        public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> item, out TKey key, out TValue value)
        {
            key = item.Key;
            value = item.Value;
        }
    }
}
