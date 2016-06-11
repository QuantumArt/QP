using System.Collections.Generic;

namespace Quantumart.QP8.Scheduler.Notification.Providers
{
	public class NotificationModel
	{
		public string Url { get; set; }
		public IEnumerable<string> OldXmlNodes { get; set; }
		public IEnumerable<string> NewXmlNodes { get; set; }
	}
}
