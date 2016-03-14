using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

namespace Quantumart.QP8.BLL.Helpers
{
	public static class VeStyleAggregationListHelper
	{
		private static string SEPARATOR = ";";
		private static string ITEM_SEPARATOR = ":";

		public static string Serialize(IEnumerable<VeStyleAggregationListItem> items)
		{
			return string.Join( SEPARATOR, items.Select(x => string.Format("{0}:{1}", x.Name, x.ItemValue)));
		}

		public static IEnumerable<VeStyleAggregationListItem> Deserialize(string str)
		{
			if (string.IsNullOrWhiteSpace(str))
				return Enumerable.Empty<VeStyleAggregationListItem>();
			return str.Split(SEPARATOR.ToCharArray()).Select(x => 
				{
					var strings = x.Split(ITEM_SEPARATOR.ToCharArray());
					return new VeStyleAggregationListItem() { Name = strings[0], ItemValue = strings[1] };
				}
			);
		}
	}
}
