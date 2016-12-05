using System;
using System.Collections.Generic;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.WebMvc.Extensions.Helpers
{
    /// <summary>
    /// Выбранный элемент списка чекбоксов
    /// </summary>
    public class QPCheckedItem
    {
        public string Value { get; set; }

        public static IEqualityComparer<T> GetComparer<T>() where T : QPCheckedItem
        {
            return new LambdaEqualityComparer<T>((f1, f2) => f1.Value.Equals(f2.Value, StringComparison.InvariantCultureIgnoreCase), f => f.Value.ToLowerInvariant().GetHashCode());
        }
    }
}
