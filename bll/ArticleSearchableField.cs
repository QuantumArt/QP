namespace Quantumart.QP8.BLL
{
    /// <summary>
    /// Информация о поле по которому можем искать статьи
    /// </summary>
    public class ArticleSearchableField
    {
        public string ID { get; set; }

        public string Name { get; set; }

        public string ColumnName { get; set; }

		public string GroupName { get; set; }

		public string ContentId { get; set; }

		public string ReferenceFieldId { get; set; }

        public ArticleFieldSearchType ArticleFieldSearchType { get; set; }

		public bool IsAll { get; set; }

		public bool IsTitle => ColumnName == "Title";

        public string Selected => (IsTitle) ? @"selected=""selected""" : string.Empty;

        public string Value => !IsAll ? ID : string.Empty;
    }
}
