using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Quantumart.QP8.BLL
{
	/// <summary>
	/// Строка результата поиска по всем статьям
	/// </summary>
	public class SearchInArticlesResultItem
	{
		public decimal Id { get; set; }
		public string Name { get; set; }
		public DateTime Created { get; set; }
		public DateTime Modified { get; set; }
		public string LastModifiedByUser { get; set; }
		public string StatusName { get; set; }

		public decimal ParentId { get; set; }
		public string ParentName { get; set; }

		public string Text { get; set; }
		public int Rank { get; set; }
	}
}