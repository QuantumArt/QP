using Newtonsoft.Json;
using Quantumart.QP8.BLL.Converters;
using System;

namespace Quantumart.QP8.BLL.ListItems
{
    public class QpPluginVersionListItem
    {
        public int Id { get; set; }

        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime Modified { get; set; }

        public string Version { get; set; }

        public string ModifiedByLogin { get; set; }
    }
}
