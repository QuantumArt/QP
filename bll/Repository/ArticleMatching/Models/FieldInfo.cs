using System;

namespace Quantumart.QP8.BLL.Repository.ArticleMatching.Models
{
	[Serializable]
	public class FieldInfo
	{
		public int Id { get; set; }
		public int? ParentId { get; set; }
		public string Name { get; set; }
		public string Type { get; set; }
		public int? ContentId { get; set; }
	}
}
