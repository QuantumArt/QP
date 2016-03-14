using System.Collections.Generic;

namespace Quantumart.QP8.BLL.Services.API.Models
{
	public class ArticleData
	{
		public ArticleData()
		{
			Fields = new List<FieldData>();
		}

		public int Id { get; set; }
		public int ContentId { get; set; }
		public List<FieldData> Fields { get; set; }

		public override string ToString()
		{
			return new
			{
				Id,
				ContentId
			}.ToString();
		}
	}
}
