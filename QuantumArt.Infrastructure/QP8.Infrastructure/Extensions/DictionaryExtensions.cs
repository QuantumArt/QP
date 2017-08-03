using System.Collections.Generic;

namespace QP8.Infrastructure.Extensions
{
    public static class DictionaryExtensions
    {
        public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> target, IDictionary<TKey, TValue> source)
        {
            Ensure.Argument.NotNull(target);
            Ensure.Argument.NotNull(source);

            foreach (var keyValuePair in source)
            {
                target.Add(keyValuePair.Key, keyValuePair.Value);
            }
        }

        public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> target, IEnumerable<KeyValuePair<TKey, TValue>> source)
        {
            Ensure.Argument.NotNull(target);
            Ensure.Argument.NotNull(source);

            foreach (var keyValuePair in source)
            {
                target.Add(keyValuePair.Key, keyValuePair.Value);
            }
        }
    }
}
