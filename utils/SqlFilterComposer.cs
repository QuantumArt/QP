using System.Collections.Generic;
using System.Linq;

namespace Quantumart.QP8.Utils
{
    public class SqlFilterComposer
    {
        public static string Compose(params string[] filters) => string.Join(" AND ", filters.Where(n => !string.IsNullOrEmpty(n)).ToArray());

        public static string Compose(IEnumerable<string> filters) => Compose(filters.ToArray());
    }
}
