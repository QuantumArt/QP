using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Flurl.Http;
using QP8.Infrastructure.Extensions;
using QP8.Infrastructure.Logging.Interfaces;
using QP8.Infrastructure.Web.Enums;
using QP8.Infrastructure.Web.Responses;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Logging;
using Quantumart.QP8.BLL.Models.NotificationSender;
using Quantumart.QP8.BLL.Services.NotificationSender;
using Quantumart.QP8.Configuration.Models;
using Quantumart.QP8.Scheduler.API;
using Quantumart.QP8.Scheduler.Notification.Data;
using Quantumart.QP8.Scheduler.Notification.Properties;

namespace Quantumart.QP8.Scheduler.Notification.Processors
{
    public class SystemNotificationProcessor : IProcessor
    {
        private readonly ILog _logger;
        private readonly PrtgErrorsHandler _prtgLogger;
        private readonly ISchedulerCustomerCollection _schedulerCustomers;
        private readonly IExternalSystemNotificationService _externalNotificationService;

        public SystemNotificationProcessor(
            ILog logger,
            PrtgErrorsHandler prtgLogger,
            ISchedulerCustomerCollection schedulerCustomers,
            IExternalSystemNotificationService externalNotificationService)
        {
            _logger = logger;
            _prtgLogger = prtgLogger;
            _schedulerCustomers = schedulerCustomers;
            _externalNotificationService = externalNotificationService;
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

            var tarantoolSentUnsentTuple = await ProcessTarantoolNotifications(customer, token);
            sentNotificationIds.AddRange(tarantoolSentUnsentTuple.Item1);
            unsentNotificationIds.AddRange(tarantoolSentUnsentTuple.Item2);

            var elasticSentUnsentTuple = await ProcessElasticNotifications(customer, token);
            sentNotificationIds.AddRange(elasticSentUnsentTuple.Item1);
            unsentNotificationIds.AddRange(elasticSentUnsentTuple.Item2);
            
            UpdateDbNotificationsData(customer.ConnectionString, sentNotificationIds, unsentNotificationIds);
            return Tuple.Create(sentNotificationIds, unsentNotificationIds);
        }

        private async Task<Tuple<List<int>, List<int>>> ProcessTarantoolNotifications(QaConfigCustomer customer, CancellationToken token)
        {
            var notificationDtos = GetTarantoolPendingNotifications(customer.ConnectionString);
            var sentNotificationIds = new List<int>();
            var unsentNotificationIds = new List<int>();
            foreach (var dto in notificationDtos)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }

                try
                {
                    var responseMessage = PushDataToHttpChannel(dto.Url, dto.Json);
                    var response = await responseMessage.ReceiveJson<JSendResponse>();
                    if (response.Status == JSendStatus.Success && response.Code == 200)
                    {
                        sentNotificationIds.Add(dto.Id);
                        _logger.Trace($"Http push notification was pushed successfuly: {response.ToJsonLog()}");
                    }
                    else
                    {
                        _logger.Warn($"Http push notification response was failed for customer code: {response.ToJsonLog()}");
                        unsentNotificationIds.Add(dto.Id);
                        break;
                    }

                    _logger.Trace($"Http push notification was pushed successfuly for customer code: {customer.CustomerName}: {response.ToJsonLog()}");
                }
                catch (Exception ex)
                {
                    unsentNotificationIds.Add(dto.Id);
                    var message = $"Exception while sending notification for customer code: {customer.CustomerName}. Notification: {dto.ToJsonLog()}";
                    _logger.Error(message, ex);
                }
            }

            return Tuple.Create(sentNotificationIds, unsentNotificationIds);
        }

        private async Task<Tuple<List<int>, List<int>>> ProcessElasticNotifications(QaConfigCustomer customer, CancellationToken token)
        {
            var notificationDtos = GetElasticPendingNotifications(customer.ConnectionString);
            var sentNotificationIds = new List<int>();
            var unsentNotificationIds = new List<int>();
            foreach (var dto in notificationDtos)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }

                try
                {
                    var responseMessage = PushDataToHttpChannel(dto.Url, dto.Json);
                    var response = await responseMessage.ReceiveString();
                    if ((await responseMessage).IsSuccessStatusCode)
                    {
                        sentNotificationIds.Add(dto.Id);
                        _logger.Trace($"Http push notification was pushed successfuly: {response}");
                    }
                    else
                    {
                        _logger.Warn($"Http push notification response was failed for customer code: {response}");
                        unsentNotificationIds.Add(dto.Id);
                        break;
                    }

                    _logger.Trace($"Http push notification was pushed successfuly for customer code: {customer.CustomerName}: {response}");
                }
                catch (Exception ex)
                {
                    unsentNotificationIds.Add(dto.Id);
                    var message = $"Exception while sending notification for customer code: {customer.CustomerName}. Notification: {dto.ToJsonLog()}";
                    _logger.Error(message, ex);
                }
            }

            return Tuple.Create(sentNotificationIds, unsentNotificationIds);
        }

        private IEnumerable<SystemNotificationDto> GetTarantoolPendingNotifications(string connection)
        {
            using (new QPConnectionScope(connection))
            {
                var notificationModels = _externalNotificationService.GetTarantoolPendingNotifications();
                return Mapper.Map<List<SystemNotificationModel>, List<SystemNotificationDto>>(notificationModels).OrderBy(n => n.TransactionLsn).ToList();
            }
        }

        private IEnumerable<SystemNotificationDto> GetElasticPendingNotifications(string connection)
        {
            using (new QPConnectionScope(connection))
            {
                var notificationModels = _externalNotificationService.GetElasticPendingNotifications();
                return Mapper.Map<List<SystemNotificationModel>, List<SystemNotificationDto>>(notificationModels).OrderBy(n => n.TransactionLsn).ToList();
            }
        }

        private void UpdateDbNotificationsData(string connection, IReadOnlyCollection<int> sentNotificationIds, IReadOnlyCollection<int> unsentNotificationIds)
        {
            using (new QPConnectionScope(connection))
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

        private static async Task<HttpResponseMessage> PushDataToHttpChannel(string urlEndpoint, string jsonData) =>
            await urlEndpoint.AllowAnyHttpStatus().WithTimeout(Settings.Default.HttpTimeout).PostStringAsync(jsonData);
    }
}
