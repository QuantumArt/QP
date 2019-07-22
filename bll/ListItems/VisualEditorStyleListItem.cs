using Newtonsoft.Json;
using Quantumart.QP8.BLL.Converters;
using System;

namespace Quantumart.QP8.BLL.ListItems
{
    public class VisualEditorStyleListItem
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public int Order { get; set; }

        public string Tag { get; set; }

        public bool IsFormat { get; set; }

        public bool IsSystem { get; set; }

        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime Created { get; set; }

        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime Modified { get; set; }

        public int LastModifiedBy { get; set; }

        public string LastModifiedByLogin { get; set; }
    }
}
