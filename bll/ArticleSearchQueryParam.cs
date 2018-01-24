namespace Quantumart.QP8.BLL
{
    public class ArticleSearchQueryParam
    {
        public ArticleFieldSearchType SearchType { get; set; }

        // ReSharper disable once InconsistentNaming
        public string FieldID { get; set; }

        // ReSharper disable once InconsistentNaming
        public string ReferenceFieldID { get; set; }

        // ReSharper disable once InconsistentNaming
        public string ContentID { get; set; }

        public string FieldColumn { get; set; }

        public object[] QueryParams { get; set; }
    }
}
