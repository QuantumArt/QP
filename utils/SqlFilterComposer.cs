using System.Collections.Generic;
using System.Linq;

namespace Quantumart.QP8.Utils
{
	public class SqlFilterComposer
	{
		public static string Compose(params string[] filters)
		{
			return string.Join(" AND ", filters.Where(n => !string.IsNullOrEmpty(n)).ToArray()); 
		}

		public static string Compose(IEnumerable<string> filters)
		{
			return Compose(filters.ToArray());
		}
	}
}
