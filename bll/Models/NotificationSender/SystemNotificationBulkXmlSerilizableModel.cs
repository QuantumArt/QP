using System.Collections.Generic;
using System.Xml.Serialization;

namespace Quantumart.QP8.BLL.Models.NotificationSender
{
    [XmlRoot("Notifications")]
    public class SystemNotificationBulkXmlSerilizableModel
    {
        [XmlElement("Notification")]
        public List<SystemNotificationModel> Notifications { get; set; }
    }
}
