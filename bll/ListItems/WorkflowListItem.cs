using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quantumart.QP8.BLL.ListItems
{
	public class WorkflowListItem
	{
		public int Id { get; set; }

		public string Status { get; set; }

		public string Name { get; set; }

		public string Description { get; set; }

		public DateTime Created { get; set; }

		public DateTime Modified { get; set; }

		public int LastModifiedBy { get; set; }

		public string LastModifiedByLogin { get; set; }
	}
}
