using System.Linq;

namespace Quantumart.QP8.BLL.Services.API.Models
{
	public class FieldData
	{
		public FieldData()
		{
			ArticleIds = new int[0];
		}

		public int Id { get; set; }

		public string Value { get; set; }

		public int[] ArticleIds { get; set; }

		public int[] ArticleIdsOrDefault => ArticleIds != null && ArticleIds.Any() ? ArticleIds : new[] { 0 };

	    public override string ToString()
		{
			return new
			{
				Id,
				Value,
				ArticleIds = ArticleIds == null ? null : string.Join(",", ArticleIds)
			}.ToString();
		}
	}
}
