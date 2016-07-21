using Quantumart.QP8.BLL.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quantumart.QP8.BLL.Services
{
	public interface IExternalNotificationService
	{
		IEnumerable<ExternalNotification> GetPendingNotifications();
		void UpdateSentNotifications(IEnumerable<int> notificationIds);
		void UpdateUnsentNotifications(IEnumerable<int> notificationIds);
		void DeleteSentNotifications();
	}

	public class ExternalNotificationService : IExternalNotificationService
	{
		public IEnumerable<ExternalNotification> GetPendingNotifications()
		{
			return ExternalNotificationRepository.GetPendingNotifications();
		}

		public void UpdateSentNotifications(IEnumerable<int> notificationIds)
		{
			ExternalNotificationRepository.UpdateSentNotifications(notificationIds);
		}

		public void UpdateUnsentNotifications(IEnumerable<int> notificationIds)
		{
			ExternalNotificationRepository.UpdateUnsentNotifications(notificationIds);
		}

		public void DeleteSentNotifications()
		{
			ExternalNotificationRepository.DeleteSentNotifications();
		}
	}
}
