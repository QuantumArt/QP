using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Scheduler.API;
using Quantumart.QP8.Scheduler.API.Extensions;
using Quantumart.QP8.Scheduler.Notification.Providers;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Quantumart.QP8.Scheduler.Notification
{
    public class NotificationProcessor : IProcessor
    {
        private const string NotificationLogMessage = "sent event {0} to {1} with status {2}";
        private const string NotificationErrorMessage = "";

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
            _logger.TraceInformation("Start sending notifications");

            foreach (var connection in _connectionStrings)
            {
                var builder = new SqlConnectionStringBuilder(connection);

                if (token.IsCancellationRequested)
                {
                    break;
                }



                ExternalNotification[] notifications = null;
                var sentNotificationIds = new List<int>();
                var unsentNotificationIds = new List<int>();

                using (var scope = new QPConnectionScope(connection))
                {
                    notifications = _externalNotificationService.GetPendingNotifications().ToArray();
                }


                var notificatioData = from g in notifications.GroupBySequence(n => new { n.Url, n.EventName, n.SiteId, n.ContentId }, n => n)
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

                foreach (var item in notificatioData)
                {
                    try
                    {
                        if (token.IsCancellationRequested)
                        {
                            break;
                        }

                        var status = await _notificationProvider.Notify(item.NotificationModel);
                        var message = $"sent event { item.NotificationModel.EventName} to {item.NotificationModel.Url} with status {status} for database {builder.InitialCatalog} on server {builder.DataSource}";

                        if (status == HttpStatusCode.OK)
                        {
                            _logger.TraceInformation(message);

                            foreach (var param in item.NotificationModel.Parameters)
                            {
                                _logger.TraceEvent(TraceEventType.Verbose, EventIdentificators.Common, $"{param.Key}: {param.Value}");
                            }

                            sentNotificationIds.AddRange(item.NotificationIds);
                        }
                        else
                        {
                            _logger.TraceEvent(TraceEventType.Warning, EventIdentificators.Common, message);
                            unsentNotificationIds.AddRange(item.NotificationIds);
                        }
                    }
                    catch (Exception ex)
                    {
                        unsentNotificationIds.AddRange(item.NotificationIds);
                        var message = $"not sent event {item.NotificationModel.EventName} to {item.NotificationModel.Url} with message {ex.Message} for database {builder.InitialCatalog} on server {builder.DataSource}";
                        _logger.TraceEvent(TraceEventType.Error, EventIdentificators.Common, message);
                        _logger.TraceData(TraceEventType.Error, EventIdentificators.Common, ex);
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

            _logger.TraceInformation("End sending notifications");
        }
        #endregion
    }
}