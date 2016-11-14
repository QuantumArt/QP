using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Quantumart.QP8.Scheduler.API.Extensions
{
    public class Grouping<TKey, TElement> : IGrouping<TKey, TElement>
    {
        public TKey Key { get; }

        public IEnumerable<TElement> Elements { get; }

        public Grouping(TKey key, IEnumerable<TElement> elements)
        {
            Key = key;
            Elements = elements;
        }

        public IEnumerator<TElement> GetEnumerator()
        {
            return Elements.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Elements.GetEnumerator();
        }
    }
}
