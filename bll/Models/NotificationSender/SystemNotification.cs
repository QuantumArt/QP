using System;

namespace Quantumart.QP8.BLL.Models.NotificationSender
{
    public class SystemNotification
    {
        public int Id { get; set; }

        public string Event { get; set; }

        public string Lsn { get; set; }

        public int? ParentEntityId { get; set; }

        public int? EntityId { get; set; }

        public string EventType { get; set; }

        public DateTime TransactionDate { get; set; }

        public DateTime Created { get; set; }

        public DateTime Modified { get; set; }

        public string Url { get; set; }

        public int Tries { get; set; }

        public string Json { get; set; }

        public bool Sent { get; set; }

    }
}
