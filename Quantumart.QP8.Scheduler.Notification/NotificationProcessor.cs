using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Scheduler.API;
using Quantumart.QP8.Scheduler.API.Extensions;
using Quantumart.QP8.Scheduler.Notification.Providers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Quantumart.QP8.Scheduler.Notification
{
	public class NotificationProcessor : IProcessor
	{
		private const string NotificationLogMessage = "send event {0} to {1} with status {2}";

		private readonly TraceSource _logger;
		private readonly IConnectionStrings _connectionStrings;
		private readonly IExternalNotificationService _externalNotificationService;
		private readonly INotificationProvider _notificationProvider;		

		public NotificationProcessor(
			TraceSource logger,
			IConnectionStrings connectionStrings,
			IExternalNotificationService externalNotificationService,
			INotificationProvider notificationProvider)
		{
			_logger = logger;
			_connectionStrings = connectionStrings;
			_externalNotificationService = externalNotificationService;
			_notificationProvider = notificationProvider;
		}

		#region IProcessor implementation
		public async Task Run(CancellationToken token)
		{
			_logger.TraceInformation("Start notification");

			foreach (var connection in _connectionStrings)
			{
				if (token.IsCancellationRequested)
				{
					break;
				}

				_logger.TraceInformation("Notification for: " + connection);

				IEnumerable<ExternalNotification> notifications = null;
				var sentNotificationIds = new List<int>();
				var unsentNotificationIds = new List<int>();

				using (var scope = new QPConnectionScope(connection))
				{
					notifications = _externalNotificationService.GetPendingNotifications();
				}

				var notificatioData = from g in notifications.GroupBySequence(n => new { n.Url, n.EventName }, n => n)
									  select new
									  {
										  NotificationModel = new NotificationModel
										  {
											  Url = g.Key.Url,
											  NewXmlNodes = g.Select(n => n.NewXml),
											  OldXmlNodes = g.Select(n => n.OldXml)
										  },
										  NotificationIds = g.Select(n => n.Id),
										  EventName = g.Key.EventName
									  };		

				foreach (var item in notificatioData)
				{
					try
					{
						if (token.IsCancellationRequested)
						{
							break;
						}

						var status = await _notificationProvider.Notify(item.NotificationModel);

						if (status == HttpStatusCode.OK)
						{
							_logger.TraceEvent(TraceEventType.Verbose, EventIdentificators.Common, NotificationLogMessage, item.EventName, item.NotificationModel.Url, status);
							sentNotificationIds.AddRange(item.NotificationIds);
						}
						else
						{
							_logger.TraceEvent(TraceEventType.Warning, EventIdentificators.Common, NotificationLogMessage, item.EventName, item.NotificationModel.Url, status);
							unsentNotificationIds.AddRange(item.NotificationIds);
						}
					}
					catch (Exception ex)
					{
						unsentNotificationIds.AddRange(item.NotificationIds);
						_logger.TraceData(TraceEventType.Warning, EventIdentificators.Common, ex);
					}
				}

				using (var scope = new QPConnectionScope(connection))
				{
					if (sentNotificationIds.Any())
					{
						_externalNotificationService.UpdateSentNotifications(sentNotificationIds);
					}

					if (unsentNotificationIds.Any())
					{
						_externalNotificationService.UpdateUnsentNotifications(unsentNotificationIds);
					}
				}
			}

			_logger.TraceInformation("End notification");
		}
		#endregion
	}
}