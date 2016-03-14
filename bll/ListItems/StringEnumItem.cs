using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quantumart.QP8.BLL.ListItems
{
	public class StringEnumItem
	{
		public string Value { get; set; }
		public string Alias { get; set; }
		public bool? IsDefault { get; set; }
		public bool Invalid { get; set; }

		internal bool GetIsDefault()
		{
			return IsDefault ?? false;
		}
	}
}
