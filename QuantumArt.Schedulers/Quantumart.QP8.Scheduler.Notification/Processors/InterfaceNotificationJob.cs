using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Models.NotificationSender;
using Quantumart.QP8.BLL.Services.NotificationSender;
using Quantumart.QP8.CommonScheduler;
using Quantumart.QP8.Configuration.Models;
using Quantumart.QP8.Scheduler.API;
using Quantumart.QP8.Scheduler.API.Extensions;
using Quantumart.QP8.Scheduler.Notification.Data;
using Quantumart.QP8.Scheduler.Notification.Providers;
using Quartz;

namespace Quantumart.QP8.Scheduler.Notification.Processors
{
    public class InterfaceNotificationJob : IJob
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private readonly ISchedulerCustomerCollection _schedulerCustomers;
        private readonly IExternalInterfaceNotificationService _externalNotificationService;
        private readonly IInterfaceNotificationProvider _notificationProvider;

        public InterfaceNotificationJob(
            ISchedulerCustomerCollection schedulerCustomers,
            IExternalInterfaceNotificationService externalNotificationService,
            IInterfaceNotificationProvider notificationProvider)
        {
            _schedulerCustomers = schedulerCustomers;
            _externalNotificationService = externalNotificationService;
            _notificationProvider = notificationProvider;
        }

        async Task IJob.Execute(IJobExecutionContext context)
        {
            Logger.Info("Start sending notifications");
            var items = _schedulerCustomers.GetItems().ToArray();
            items = JobHelpers.FilterCustomers(items, context.MergedJobDataMap);
            var token = context.CancellationToken;
            foreach (var customer in items)
            {
                try
                {
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }

                    var notificationIdsStatus = await ProcessCustomer(customer, token);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "There was an error on customer code {customerCode}", customer.CustomerName);
                }
            }

            Logger.Info("End sending notifications");
        }

        private async Task<Tuple<List<int>, List<int>>> ProcessCustomer(QaConfigCustomer customer, CancellationToken token)
        {
            using (new QPConnectionScope(customer.ConnectionString, customer.DbType))
            {
                var sentNotificationIds = new List<int>();
                var unsentNotificationIds = new List<int>();
                var notificationsViewModels = GetPendingNotificationsViewModels();
                foreach (var notificationVm in notificationsViewModels)
                {
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }

                    try
                    {
                        var notificationIdsStatus = await SendNotificationData(notificationVm, customer.CustomerName);
                        sentNotificationIds.AddRange(notificationIdsStatus.Item1);
                        unsentNotificationIds.AddRange(notificationIdsStatus.Item2);
                    }
                    catch (Exception ex)
                    {
                        unsentNotificationIds.AddRange(notificationVm.NotificationsIds);
                        Logger.Error(ex,
                            "Exception while sending event {eventName} to {url} for customer code {customerCode}",
                            notificationVm.NotificationModel.EventName,
                            notificationVm.NotificationModel.Url,
                            customer.CustomerName);
                    }
                }

                UpdateDbNotificationsData(sentNotificationIds, unsentNotificationIds);
                return Tuple.Create(sentNotificationIds, unsentNotificationIds);
            }
        }

        private IEnumerable<InterfaceNotificationViewModel> GetPendingNotificationsViewModels()
        {
            IEnumerable<ExternalNotificationModel> notifications;
            using (new QPConnectionScope())
            {
                notifications = _externalNotificationService.GetPendingNotifications();
            }

            return notifications
                .GroupBySequence(n => new { n.Url, n.EventName, n.SiteId, n.ContentId }, n => n)
                .Select(group => new InterfaceNotificationViewModel
                {
                    NotificationModel = new InterfaceNotificationModel
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

        private async Task<Tuple<List<int>, List<int>>> SendNotificationData(InterfaceNotificationViewModel notificationVm, string customerName)
        {
            var sentNotificationIds = new List<int>();
            var unsentNotificationIds = new List<int>();
            var status = await _notificationProvider.Notify(notificationVm.NotificationModel);
            if (status == HttpStatusCode.OK)
            {
                Logger.Info(
                    "Sent event {eventName} to {url} with status {statusCode} for customer code {customerCode}",
                    notificationVm.NotificationModel.EventName,
                    notificationVm.NotificationModel.Url,
                    status,
                    customerName
                );
                foreach (var param in notificationVm.NotificationModel.Parameters)
                {
                    Logger.Info($"{param.Key}: {param.Value}");
                }

                sentNotificationIds.AddRange(notificationVm.NotificationsIds);
            }
            else
            {
                Logger.Warn(
                    "Not sent event {eventName} to {url} with status {statusCode} for customer code {customerCode}",
                    notificationVm.NotificationModel.EventName,
                    notificationVm.NotificationModel.Url,
                    status,
                    customerName
                );
                unsentNotificationIds.AddRange(notificationVm.NotificationsIds);
            }

            return Tuple.Create(sentNotificationIds, unsentNotificationIds);
        }

        private void UpdateDbNotificationsData(IReadOnlyCollection<int> sentNotificationIds, IReadOnlyCollection<int> unsentNotificationIds)
        {
            using (new QPConnectionScope())
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
