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
using Quantumart.QP8.Constants.Cdc.Enums;
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
                    prtgErrorsHandlerVm.IncrementTasksQueueCount(notificationIdsStatus.prtgUnsentNotificationIds.Count);
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

        private async Task<(List<int> prtgSentNotificationIds, List<int> prtgUnsentNotificationIds)> ProcessCustomer(QaConfigCustomer customer, CancellationToken token)
        {
            var prtgSentNotificationIds = new List<int>();
            var prtgUnsentNotificationIds = new List<int>();

            var tarantoolSentUnsentTuple = await ProcessTarantoolNotifications(customer, token);
            prtgSentNotificationIds.AddRange(tarantoolSentUnsentTuple.sentNotificationIds);
            prtgUnsentNotificationIds.AddRange(tarantoolSentUnsentTuple.unsentNotificationIds);

            var elasticSentUnsentTuple = await ProcessElasticNotifications(customer, token);
            prtgSentNotificationIds.AddRange(elasticSentUnsentTuple.sentNotificationIds);
            prtgUnsentNotificationIds.AddRange(elasticSentUnsentTuple.unsentNotificationIds);

            UpdateDbNotificationsData(customer.ConnectionString, tarantoolSentUnsentTuple);
            UpdateDbNotificationsData(customer.ConnectionString, elasticSentUnsentTuple);

            return (prtgSentNotificationIds, prtgUnsentNotificationIds);
        }

        private async Task<(List<int> sentNotificationIds, List<int> unsentNotificationIds, string lastExceptionMessage)> ProcessTarantoolNotifications(QaConfigCustomer customer, CancellationToken token)
        {
            var notificationDtos = GetPendingNotifications(customer.ConnectionString, CdcProviderName.Tarantool.ToString()).ToList();
            var sentNotificationIds = new List<int>();

            string httpNotSendedReason = null;
            var notSendedDtosQueue = new Queue<SystemNotificationDto>(notificationDtos);
            while (notSendedDtosQueue.Any() && !token.IsCancellationRequested)
            {
                var dto = notSendedDtosQueue.Peek();
                try
                {
                    var responseMessage = PushDataToHttpChannel(dto.Url, dto.Json, customer.CustomerName);
                    var response = await responseMessage.ReceiveJson<JSendResponse>();
                    if (response.Status != JSendStatus.Success || response.Code != 200)
                    {
                        httpNotSendedReason = response.ToJsonLog();
                        _logger.Warn($"Http push notification response was failed for customer code: {customer.CustomerName}: {response.ToJsonLog()}");
                        break;
                    }

                    sentNotificationIds.Add(notSendedDtosQueue.Dequeue().Id);
                    _logger.Trace($"Http push notification was pushed successfuly for customer code: {customer.CustomerName}: {response.ToJsonLog()}");
                }
                catch (Exception ex)
                {
                    httpNotSendedReason = ex.Dump();
                    _logger.Error($"Exception while sending notification for customer code: {customer.CustomerName}. Notification: {dto.ToJsonLog()}", ex);
                    break;
                }
            }

            return (sentNotificationIds, notSendedDtosQueue.Select(dto => dto.Id).ToList(), httpNotSendedReason);
        }

        private async Task<(List<int> sentNotificationIds, List<int> unsentNotificationIds, string lastExceptionMessage)> ProcessElasticNotifications(QaConfigCustomer customer, CancellationToken token)
        {
            var notificationDtos = GetPendingNotifications(customer.ConnectionString, CdcProviderName.Elastic.ToString());
            var sentNotificationIds = new List<int>();

            string httpNotSendedReason = null;
            var notSendedDtosQueue = new Queue<SystemNotificationDto>(notificationDtos);
            while (notSendedDtosQueue.Any() && !token.IsCancellationRequested)
            {
                var dto = notSendedDtosQueue.Peek();
                try
                {
                    var responseMessage = PushDataToHttpChannel(dto.Url, dto.Json, customer.CustomerName);
                    var response = await responseMessage.ReceiveString();
                    if (!(await responseMessage).IsSuccessStatusCode)
                    {
                        httpNotSendedReason = response;
                        _logger.Warn($"Http push notification response was failed for customer code: {customer.CustomerName}: {response}");
                        break;
                    }

                    sentNotificationIds.Add(notSendedDtosQueue.Dequeue().Id);
                    _logger.Trace($"Http push notification was pushed successfuly for customer code: {customer.CustomerName}: {response}");
                }
                catch (Exception ex)
                {
                    httpNotSendedReason = ex.Dump();
                    _logger.Error($"Exception while sending notification for customer code: {customer.CustomerName}. Notification: {dto.ToJsonLog()}", ex);
                    break;
                }
            }

            return (sentNotificationIds, notSendedDtosQueue.Select(dto => dto.Id).ToList(), httpNotSendedReason);
        }

        private IEnumerable<SystemNotificationDto> GetPendingNotifications(string connection, string providerName)
        {
            using (new QPConnectionScope(connection))
            {
                var notificationModels = _externalNotificationService.GetPendingNotifications(providerName);
                return Mapper.Map<List<SystemNotificationModel>, List<SystemNotificationDto>>(notificationModels).OrderBy(n => n.TransactionLsn).ToList();
            }
        }

        private void UpdateDbNotificationsData(string connection, (List<int> sentNotificationIds, List<int> unsentNotificationIds, string lastExceptionMessage) sentUnsentTuple)
        {
            using (new QPConnectionScope(connection))
            {
                if (sentUnsentTuple.sentNotificationIds.Any())
                {
                    _externalNotificationService.UpdateSentNotifications(sentUnsentTuple.sentNotificationIds);
                }

                if (sentUnsentTuple.unsentNotificationIds.Any())
                {
                    _externalNotificationService.UpdateUnsentNotifications(new List<int> { sentUnsentTuple.unsentNotificationIds.First() }, sentUnsentTuple.lastExceptionMessage);
                }
            }
        }

        private static async Task<HttpResponseMessage> PushDataToHttpChannel(string urlEndpoint, string jsonData, string customerCode) =>
            await urlEndpoint.Replace("{customercode}", customerCode).AllowAnyHttpStatus().WithTimeout(Settings.Default.HttpTimeout).PostStringAsync(jsonData);
    }
}
