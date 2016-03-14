using Quantumart.QP8.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quantumart.QP8.BLL.Helpers
{
	public static class SortingItemHelper
	{
		private static char[] delimeter = {','};		

		public static IEnumerable<SortingItem> Deserialize(string stringToParse)
		{
			if(!string.IsNullOrWhiteSpace(stringToParse) && stringToParse != "[]")
				return stringToParse.Split(delimeter).Select(x => SortingItem.CreatefromString(x));
			else
				return Enumerable.Empty<SortingItem>();
		}


		public static string Serialize(IEnumerable<SortingItem> list)
		{
			var arr = list.Select(x => string.Format("[{0}] {1}", x.Field, x.ShortSortingOrderName)).ToArray();
			return string.Join(",", arr);
		}
	}
}
