using Newtonsoft.Json;
using Quantumart.QP8.BLL.Converters;
using System;

namespace Quantumart.QP8.BLL.ListItems
{
    public class WorkflowListItem
    {
        public int Id { get; set; }

        public string Status { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime Created { get; set; }

        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime Modified { get; set; }

        public int LastModifiedBy { get; set; }

        public string LastModifiedByLogin { get; set; }
    }
}
