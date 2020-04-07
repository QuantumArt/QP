using Newtonsoft.Json;
using Quantumart.QP8.BLL.Converters;
using System;

namespace Quantumart.QP8.BLL.ListItems
{
    public class ArticleListItem
    {
        public decimal Id { get; set; }

        public decimal ParentId { get; set; }

        public string Title { get; set; }

        public bool IsPermanentLock { get; set; }

        public string SiteName { get; set; }

        public string ContentName { get; set; }

        public string StatusName { get; set; }

        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime Created { get; set; }

        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime Modified { get; set; }

        public string LastModifiedByUser { get; set; }
    }
}
