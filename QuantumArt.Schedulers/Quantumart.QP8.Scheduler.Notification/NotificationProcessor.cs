using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using QP8.Infrastructure.Logging;
using QP8.Infrastructure.Logging.Interfaces;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Scheduler.API;
using Quantumart.QP8.Scheduler.API.Extensions;
using Quantumart.QP8.Scheduler.Notification.Providers;

namespace Quantumart.QP8.Scheduler.Notification
{
    public class NotificationProcessor : IProcessor, IDisposable
    {
        private readonly ILog _logger;
        private readonly ISchedulerCustomers _schedulerCustomers;
        private readonly IExternalNotificationService _externalNotificationService;
        private readonly INotificationProvider _notificationProvider;

        public NotificationProcessor(
            ILog logger,
            ISchedulerCustomers schedulerCustomers,
            IExternalNotificationService externalNotificationService,
            INotificationProvider notificationProvider)
        {
            _logger = logger;
            _schedulerCustomers = schedulerCustomers;
            _externalNotificationService = externalNotificationService;
            _notificationProvider = notificationProvider;
        }

        public async Task Run(CancellationToken token)
        {
            _logger.Info("Start sending notifications");
            foreach (var customer in _schedulerCustomers)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }

                ExternalNotification[] notifications;
                var sentNotificationIds = new List<int>();
                var unsentNotificationIds = new List<int>();
                using (new QPConnectionScope(customer.ConnectionString))
                {
                    notifications = _externalNotificationService.GetPendingNotifications().ToArray();
                }

                var notificationData = from g in notifications.GroupBySequence(n => new { n.Url, n.EventName, n.SiteId, n.ContentId }, n => n)
                                       select new
                                       {
                                           NotificationModel = new NotificationModel
                                           {
                                               Url = g.Key.Url,
                                               SiteId = g.Key.SiteId,
                                               ContentId = g.Key.ContentId,
                                               EventName = g.Key.EventName,
                                               Ids = g.Select(n => n.ArticleId),
                                               NewXmlNodes = g.Select(n => n.NewXml),
                                               OldXmlNodes = g.Select(n => n.OldXml)
                                           },
                                           NotificationIds = g.Select(n => n.Id)
                                       };

                foreach (var item in notificationData)
                {
                    try
                    {
                        if (token.IsCancellationRequested)
                        {
                            break;
                        }

                        var status = await _notificationProvider.Notify(item.NotificationModel);
                        var message = $"Sent event { item.NotificationModel.EventName} to {item.NotificationModel.Url} with status {status} for customer code: {customer.CustomerName}";
                        if (status == HttpStatusCode.OK)
                        {
                            _logger.Info(message);
                            foreach (var param in item.NotificationModel.Parameters)
                            {
                                _logger.Info($"{param.Key}: {param.Value}");
                            }

                            sentNotificationIds.AddRange(item.NotificationIds);
                        }
                        else
                        {
                            _logger.Info(message);
                            unsentNotificationIds.AddRange(item.NotificationIds);
                        }
                    }
                    catch (Exception ex)
                    {
                        unsentNotificationIds.AddRange(item.NotificationIds);
                        var message = $"Exception while sending event {item.NotificationModel.EventName} to {item.NotificationModel.Url} with message {ex.Message} for customer code: {customer.CustomerName}";
                        _logger.Error(message, ex);
                    }
                }

                using (new QPConnectionScope(customer.ConnectionString))
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

            _logger.Info("End sending notifications");
        }

        public void Dispose()
        {
            Logger.Log.Dispose();
        }
    }
}
