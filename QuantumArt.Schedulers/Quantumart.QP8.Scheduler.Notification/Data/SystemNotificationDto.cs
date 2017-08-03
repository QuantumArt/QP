using System;
using Newtonsoft.Json;

namespace Quantumart.QP8.Scheduler.Notification.Data
{
    internal class SystemNotificationDto
    {
        public int Id { get; set; }

        public string EventName { get; set; }

        public string TransactionLsn { get; set; }

        public DateTime TransactionDate { get; set; }

        public string EntityType { get; set; }

        public int? EntityId { get; set; }

        public int? ParentEntityId { get; set; }

        public string Url { get; set; }

        [JsonIgnore]
        public string Json { get; set; }

        public DateTime Created { get; set; }

        public DateTime Modified { get; set; }

        public int Tries { get; set; }

        public bool Sent { get; set; }
    }
}
