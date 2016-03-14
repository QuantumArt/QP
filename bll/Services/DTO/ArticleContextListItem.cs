using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quantumart.QP8.BLL.Services.DTO
{
	public class ArticleContextListItem
	{
		public bool HasHierarchy { get; set; }
		public int FieldId { get; set; }
		public Dictionary<string, string> Ids { get; set; }
	}
}