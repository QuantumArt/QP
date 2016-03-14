namespace Quantumart.QP8.DAL.DTO
{
    public class ArticleFullTextSearchParameter
    {
        public bool? HasError { get; set; }

        public string FieldIdList { get; set; }

        public string QueryString { get; set; }

		public string RawQueryString { get; set; }

        public int SearchResultLimit { get; set; }
    }
}
