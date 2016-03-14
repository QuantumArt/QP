using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quantumart.QP8.BLL.Services.DTO
{
	public class ArticleVariationListItem
	{
		public string Context { get; set; }
		public int Id { get; set; }
		public Dictionary<string, string> FieldValues { get; set; }
	}
}
