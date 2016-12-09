using System;
using System.Linq;

namespace Quantumart.QP8.BLL.Repository.ArticleMatching.Conditions
{
    public static class ConditionExtensions
    {
        public static ConditionBase Select<T>(this ConditionBase source, Func<T, ConditionBase> selector)
            where T : ConditionBase
        {
            if (source is T)
            {
                source = selector((T)source);
            }

            source.Conditions = source.Conditions
                .Select(c => c.Select(selector))
                .ToArray();

            return source;
        }

        public static ConditionBase Update<T>(this ConditionBase source, Action<T> update)
            where T : ConditionBase
        {
            var sourceT = source as T;
            if (sourceT != null)
            {
                update(sourceT);
            }

            foreach (var condition in source.Conditions)
            {
                condition.Update(update);
            }

            return source;
        }

        public static ConditionBase Where(this ConditionBase source, Predicate<ConditionBase> predicate)
        {
            source.Conditions = source.Conditions.Select(c => c.Where(predicate)).Where(c => c != null).ToArray();
            return predicate(source) ? source : null;
        }

        public static ConditionBase Where<T>(this ConditionBase source, Predicate<T> predicate)
            where T : ConditionBase
        {
            source.Conditions = source.Conditions.Select(c => c.Where<T>(predicate)).Where(c => c != null).ToArray();
            var sourceT = source as T;
            if (sourceT != null)
            {
                return predicate(sourceT) ? source : null;
            }

            return source;
        }
    }
}
