using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Quantumart.QP8.BLL.Helpers;

namespace Quantumart.QP8.BLL.ListItems
{
	public class CustomActionListItem
	{
		public int Id { get; set; }

		public string Name { get; set; }		

		public string Url { get; set; }
		
		public int Order { get; set; }

		public string ActionTypeName { get; set; }

		public string EntityTypeName { get; set; }

		public DateTime Created { get; set; }

		public DateTime Modified { get; set; }

		public int LastModifiedByUserId { get; set; }

		public string LastModifiedByUser { get; set; }

	}
}