using Newtonsoft.Json;
using Quantumart.QP8.BLL.Converters;
using System;

namespace Quantumart.QP8.BLL.ListItems
{
    public class NotificationListItem
    {
        public int Id { get; set; }

        public string Name { get; set; }

        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime Created { get; set; }

        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime Modified { get; set; }

        public int LastModifiedBy { get; set; }

        public string LastModifiedByLogin { get; set; }

        public bool ForCreate { get; set; }

        public bool ForModify { get; set; }

        public bool ForRemove { get; set; }

        public bool ForStatusChanged { get; set; }

        public bool ForStatusPartiallyChanged { get; set; }

        public bool ForDelayedPublication { get; set; }

        public bool IsExternal { get; set; }

        public bool NoEmail { get; set; }

        public int? FieldId { get; set; }

        /// <summary>
        /// Запрос по требованию
        /// </summary>
        public bool ForFrontend { get; set; }

        public string Receiver { get; set; }
    }
}
