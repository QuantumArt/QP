using System.Collections.Generic;

namespace Quantumart.QP8.BLL.Services.DTO
{
	public class ArticleVariationListItem
	{
		public string Context { get; set; }
		public int Id { get; set; }
		public Dictionary<string, string> FieldValues { get; set; }
	}
}
