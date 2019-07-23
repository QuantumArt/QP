using System;
using Newtonsoft.Json;
using Quantumart.QP8.BLL.Converters;

namespace Quantumart.QP8.BLL.ListItems
{
    public class UserGroupListItem
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public bool SharedArticles { get; set; }

        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime Created { get; set; }

        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime Modified { get; set; }

        public int LastModifiedByUserId { get; set; }

        public string LastModifiedByUser { get; set; }
    }
}
