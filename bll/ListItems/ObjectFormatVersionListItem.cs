using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quantumart.QP8.BLL.ListItems
{
	public class ObjectFormatVersionListItem
	{
		public int Id { get; set; }

		public DateTime Modified { get; set; }

		public string Description { get; set; }

		public string ModifiedByLogin { get; set; }
	}
}
