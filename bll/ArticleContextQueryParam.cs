namespace Quantumart.QP8.BLL
{
	/// <summary>
	/// Параметр запроса для поиска статей
	/// </summary>
	public class ArticleContextQueryParam
	{
		public string Name { get; set; }

		public string Value { get; set; }

		public int FieldId { get; set; }

	}
}