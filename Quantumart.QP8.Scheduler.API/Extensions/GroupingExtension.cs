using System;
using System.Collections.Generic;
using System.Linq;

namespace Quantumart.QP8.Scheduler.API.Extensions
{
    public static class GroupingExtension
    {
        public static IEnumerable<IGrouping<TKey, TElement>> GroupBySequence<TSource, TKey, TElement>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector)
        {
            if (source.Any())
            {
                var currentKey = keySelector(source.First());
                var foundItems = new List<TElement>();

                foreach (var item in source)
                {
                    var key = keySelector(item);

                    if (!currentKey.Equals(key))
                    {
                        yield return new Grouping<TKey, TElement>(currentKey, foundItems);
                        currentKey = key;
                        foundItems = new List<TElement>();
                    }

                    foundItems.Add(elementSelector(item));
                }

                if (foundItems.Any())
                {
                    yield return new Grouping<TKey, TElement>(currentKey, foundItems);
                }
            }
        }
    }
}
