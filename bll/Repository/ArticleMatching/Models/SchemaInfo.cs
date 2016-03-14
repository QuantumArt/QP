namespace Quantumart.QP8.BLL.Repository.ArticleMatching.Models
{
	internal class SchemaInfo
	{
		public int RootContentId { get; set; }
		public int ContentId { get; set; }
		public int? RefContentId { get; set; }
		public int? LinkId { get; set; }
		public string Alias { get; set; }
		public string Field { get; set; }
		public string BackwardField { get; set; }
		public int AttributeTypeId { get; set; }
		public string DataType { get; set; }
	}
}
