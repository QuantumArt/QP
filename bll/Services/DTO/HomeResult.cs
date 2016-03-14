using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quantumart.QP8.BLL.Services.DTO
{
	public class HomeResult
	{
		public IEnumerable<ListItem> Sites { get; set; }

		public User CurrentUser { get; set; }

		public int ApprovalCount { get; set; }

		public int LockedCount { get; set; }

	}
}
