using System;

namespace Quantumart.QP8.BLL.Models.NotificationSender
{
    public class ExternalNotificationModel
    {
        public int Id { get; set; }

        public string EventName { get; set; }

        public DateTime Created { get; set; }

        public DateTime Modified { get; set; }

        public int ArticleId { get; set; }

        public string Url { get; set; }

        public int Tries { get; set; }

        public string NewXml { get; set; }

        public string OldXml { get; set; }

        public bool Sent { get; set; }

        public int? ContentId { get; set; }

        public int? SiteId { get; set; }
    }
}
