using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using QP8.Infrastructure.Logging.Interfaces;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Logging;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Configuration.Models;
using Quantumart.QP8.Scheduler.API;
using Quantumart.QP8.Scheduler.API.Extensions;
using Quantumart.QP8.Scheduler.Notification.Data;
using Quantumart.QP8.Scheduler.Notification.Providers;

namespace Quantumart.QP8.Scheduler.Notification
{
    public class NotificationProcessor : IProcessor
    {
        private readonly ILog _logger;
        private readonly PrtgErrorsHandler _prtgLogger;
        private readonly ISchedulerCustomers _schedulerCustomers;
        private readonly IExternalNotificationService _externalNotificationService;
        private readonly INotificationProvider _notificationProvider;

        public NotificationProcessor(
            ILog logger,
            PrtgErrorsHandler prtgLogger,
            ISchedulerCustomers schedulerCustomers,
            IExternalNotificationService externalNotificationService,
            INotificationProvider notificationProvider)
        {
            _logger = logger;
            _prtgLogger = prtgLogger;
            _schedulerCustomers = schedulerCustomers;
            _externalNotificationService = externalNotificationService;
            _notificationProvider = notificationProvider;
        }

        public async Task Run(CancellationToken token)
        {
            _logger.Info("Start sending notifications");
            var prtgErrorsHandlerVm = new PrtgErrorsHandlerViewModel(_schedulerCustomers.ToList());
            foreach (var customer in _schedulerCustomers)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }

                try
                {
                    var notificationIdsStatus = await ProcessCustomer(customer, token);
                    prtgErrorsHandlerVm.IncrementTasksQueueCount(notificationIdsStatus.Item2.Count);
                }
                catch (Exception ex)
                {
                    ex.Data.Clear();
                    ex.Data.Add("CustomerCode", customer.CustomerName);
                    _logger.Error($"There was an error on customer code: {customer.CustomerName}", ex);
                    prtgErrorsHandlerVm.EnqueueNewException(ex);
                }
            }

            _prtgLogger.LogMessage(prtgErrorsHandlerVm);
            _logger.Info("End sending notifications");
        }

        private async Task<Tuple<List<int>, List<int>>> ProcessCustomer(QaConfigCustomer customer, CancellationToken token)
        {
            var sentNotificationIds = new List<int>();
            var unsentNotificationIds = new List<int>();
            var notificationsViewModels = GetPendingNotificationsViewModels(customer);
            foreach (var notificationVm in notificationsViewModels)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }

                try
                {
                    var notificationIdsStatus = await SendNotificationData(notificationVm, customer);
                    sentNotificationIds.AddRange(notificationIdsStatus.Item1);
                    unsentNotificationIds.AddRange(notificationIdsStatus.Item2);
                }
                catch (Exception ex)
                {
                    unsentNotificationIds.AddRange(notificationVm.NotificationsIds);
                    var message = $"Exception while sending event {notificationVm.NotificationModel.EventName} to {notificationVm.NotificationModel.Url} with message {ex.Message} for customer code: {customer.CustomerName}";
                    _logger.Error(message, ex);
                }
            }

            UpdateDbNotificationsData(customer, sentNotificationIds, unsentNotificationIds);
            return Tuple.Create(sentNotificationIds, unsentNotificationIds);
        }

        private IEnumerable<NotificationViewModel> GetPendingNotificationsViewModels(QaConfigCustomer customer)
        {
            IEnumerable<ExternalNotification> notifications;
            using (new QPConnectionScope(customer.ConnectionString))
            {
                notifications = _externalNotificationService.GetPendingNotifications();
            }

            return notifications
                .GroupBySequence(n => new { n.Url, n.EventName, n.SiteId, n.ContentId }, n => n)
                .Select(group => new NotificationViewModel
                {
                    NotificationModel = new NotificationModel
                    {
                        Url = group.Key.Url,
                        SiteId = group.Key.SiteId,
                        ContentId = group.Key.ContentId,
                        EventName = group.Key.EventName,
                        Ids = group.Select(n => n.ArticleId),
                        NewXmlNodes = group.Select(n => n.NewXml),
                        OldXmlNodes = group.Select(n => n.OldXml)
                    },
                    NotificationsIds = group.Select(n => n.Id).ToList()
                });
        }

        private async Task<Tuple<List<int>, List<int>>> SendNotificationData(NotificationViewModel notificationVm, QaConfigCustomer customer)
        {
            var sentNotificationIds = new List<int>();
            var unsentNotificationIds = new List<int>();
            var status = await _notificationProvider.Notify(notificationVm.NotificationModel);
            var message = $"Sent event { notificationVm.NotificationModel.EventName} to {notificationVm.NotificationModel.Url} with status {status} for customer code: {customer.CustomerName}";
            if (status == HttpStatusCode.OK)
            {
                _logger.Info(message);
                foreach (var param in notificationVm.NotificationModel.Parameters)
                {
                    _logger.Info($"{param.Key}: {param.Value}");
                }

                sentNotificationIds.AddRange(notificationVm.NotificationsIds);
            }
            else
            {
                _logger.Info(message);
                unsentNotificationIds.AddRange(notificationVm.NotificationsIds);
            }

            return Tuple.Create(sentNotificationIds, unsentNotificationIds);
        }

        private void UpdateDbNotificationsData(QaConfigCustomer customer, IReadOnlyCollection<int> sentNotificationIds, IReadOnlyCollection<int> unsentNotificationIds)
        {
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
    }
}
