using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Quantumart.QP8.BLL
{
	/// <summary>
	/// Параметр запроса для поиска статей
	/// </summary>
	public class ArticleContextSearchBlockItem
	{
		public string ContentName { get; set; }

		public int ContentId { get; set; }

		public int FieldId { get; set; }

	}
}