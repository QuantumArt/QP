using System.Collections.Generic;
using System.Linq;
using Quantumart.QP8.BLL.Repository;

namespace Quantumart.QP8.BLL.Services.API
{
	public class ExternalNotificationService
	{
		private readonly NotificationPushRepository _notificationPushRepository;

		public ExternalNotificationService()
		{
			_notificationPushRepository = new NotificationPushRepository();
		}

		public void PrepareNotifications(int contentId, IEnumerable<int> articleIds, IEnumerable<string> codes)
		{
			_notificationPushRepository.PrepareNotifications(contentId, articleIds?.ToArray(), codes?.ToArray());
		}

		public void PrepareNotifications(Article article, IEnumerable<string> codes)
		{
			_notificationPushRepository.PrepareNotifications(article, codes?.ToArray());
		}

		public void PrepareNotifications(int contentId, IEnumerable<int> ids, string code)
		{
			_notificationPushRepository.PrepareNotifications(contentId, ids?.ToArray(), code);
		}

		public void SendNotifications()
		{
			_notificationPushRepository.SendNotifications();
		}
	}
}
