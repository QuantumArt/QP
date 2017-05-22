using System;
using System.Collections.Generic;
using System.Linq;

namespace QP8.Infrastructure.Extensions
{
    public static class DictionaryExtensions
    {
        public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> target, IDictionary<TKey, TValue> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            foreach (var keyValuePair in source)
            {
                target.Add(keyValuePair.Key, keyValuePair.Value);
            }
        }

        public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> target, IEnumerable<KeyValuePair<TKey, TValue>> source)
        {
            target.AddRange(source.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
        }
    }
}
