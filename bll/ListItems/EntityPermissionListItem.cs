using Newtonsoft.Json;
using Quantumart.QP8.BLL.Converters;
using System;

namespace Quantumart.QP8.BLL.ListItems
{
    /// <summary>
    /// Элемент списка доступов к сущности
    /// </summary>
    public class EntityPermissionListItem
    {
        public int Id { get; set; }

        public string UserLogin { get; set; }

        public string GroupName { get; set; }

        public string LevelName { get; set; }

        public bool PropagateToItems { get; set; }

        public bool Hide { get; set; }

        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime Created { get; set; }

        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime Modified { get; set; }

        public int LastModifiedByUserId { get; set; }

        public string LastModifiedByUser { get; set; }
    }
}
