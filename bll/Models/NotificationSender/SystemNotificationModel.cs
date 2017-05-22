using System;

namespace Quantumart.QP8.BLL.Models.NotificationSender
{
    public class SystemNotificationModel
    {
        public int Id { get; set; }

        public string TransactionLsn { get; set; }

        public DateTime TransactionDate { get; set; }

        public string Url { get; set; }

        public string Json { get; set; }

        public DateTime Created { get; set; }

        public DateTime Modified { get; set; }

        public int Tries { get; set; }

        public bool Sent { get; set; }
    }
}
