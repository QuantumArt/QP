using Newtonsoft.Json;
using Quantumart.QP8.BLL.Converters;
using System;

namespace Quantumart.QP8.BLL
{
    public class SearchInArticlesResultItem
    {
        public decimal Id { get; set; }

        public string Name { get; set; }

        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime Created { get; set; }

        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime Modified { get; set; }

        public string LastModifiedByUser { get; set; }

        public string StatusName { get; set; }

        public decimal ParentId { get; set; }

        public string ParentName { get; set; }

        public string Text { get; set; }

        public int Rank { get; set; }

        public decimal Archive { get; set; }

        public string ActionCode => Archive == 0 ? Constants.ActionCode.EditArticle : Constants.ActionCode.ViewArchiveArticle;

        public string EntityTypeCode => Archive == 0 ? Constants.EntityTypeCode.Article : Constants.EntityTypeCode.ArchiveArticle;
    }
}
