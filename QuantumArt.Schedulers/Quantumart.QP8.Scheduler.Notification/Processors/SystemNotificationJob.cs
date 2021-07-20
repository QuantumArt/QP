using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Flurl.Http;
using Newtonsoft.Json;
using NLog;
using NLog.Fluent;
using QP8.Infrastructure.Extensions;
using QP8.Infrastructure.Web.Enums;
using QP8.Infrastructure.Web.Responses;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Models.NotificationSender;
using Quantumart.QP8.BLL.Services.NotificationSender;
using Quantumart.QP8.Configuration.Models;
using Quantumart.QP8.Constants.Cdc.Enums;
using Quantumart.QP8.Scheduler.API;
using Quantumart.QP8.Scheduler.Notification.Data;
using Quantumart.QP8.Scheduler.Notification.Properties;
using Quartz;

namespace Quantumart.QP8.Scheduler.Notification.Processors
{
    public class SystemNotificationJob: IJob
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private readonly ISchedulerCustomerCollection _schedulerCustomers;
        private readonly IExternalSystemNotificationService _externalNotificationService;

        public SystemNotificationJob(
            ISchedulerCustomerCollection schedulerCustomers,
            IExternalSystemNotificationService externalNotificationService)
        {
            _schedulerCustomers = schedulerCustomers;
            _externalNotificationService = externalNotificationService;
        }

         async Task IJob.Execute(IJobExecutionContext context)
        {
            Logger.Info("Start sending notifications");
            var items = _schedulerCustomers.GetItems();
            var token = context.CancellationToken;
            foreach (var customer in items)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }

                try
                {
                    await ProcessCustomer(customer, token);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "There was an error on customer code: {customerCode}", customer.CustomerName);
                }
            }

            Logger.Info("End sending notifications");
        }

        private async Task ProcessCustomer(QaConfigCustomer customer, CancellationToken token)
        {
            using (new QPConnectionScope(customer.ConnectionString, customer.DbType))
            {
                var tarantoolSentUnsentTuple = await ProcessTarantoolNotifications(customer, token);
                UpdateDbNotificationsData(tarantoolSentUnsentTuple);

                var elasticSentUnsentTuple = await ProcessElasticNotifications(customer, token);
                UpdateDbNotificationsData(elasticSentUnsentTuple);
            }
        }

        private async Task<(List<int> sentNotificationIds, List<int> unsentNotificationIds, string lastExceptionMessage)> ProcessTarantoolNotifications(QaConfigCustomer customer, CancellationToken token)
        {
            var notificationDtos = GetPendingNotifications(CdcProviderName.Tarantool.ToString()).ToList();
            var sentNotificationIds = new List<int>();

            string httpNotSendedReason = null;
            var notSendedDtosQueue = new Queue<SystemNotificationDto>(notificationDtos);

            while (notSendedDtosQueue.Any() && token.IsCancellationRequested)
            {
                var dto = notSendedDtosQueue.Peek();
                Task<HttpResponseMessage> responseMessage = null;

                try
                {
                    responseMessage = PushDataToHttpChannel(dto.Url, dto.Json, customer.CustomerName);
                    var response = await responseMessage.ReceiveJson<JSendResponse>();
                    if (response.Status != JSendStatus.Success || response.Code != 200)
                    {
                        httpNotSendedReason = response.ToJsonLog();
                        Logger.Warn($"Http push notification response was failed for customer code: {customer.CustomerName}: {response.ToJsonLog()}");
                        break;
                    }

                    sentNotificationIds.Add(notSendedDtosQueue.Dequeue().Id);
                    Logger.Trace($"Http push notification was pushed successfuly for customer code: {customer.CustomerName}: {response.ToJsonLog()}");
                }
                catch (Exception ex) when (ex is JsonReaderException)
                {
                    var responseBodyMessage = $"Response body: {await responseMessage.ReceiveString()}.";
                    httpNotSendedReason = $"Exception while parsing response. {responseBodyMessage}";
                    Logger.Error(ex, $"Exception while parsing response for customer code: {customer.CustomerName}. {responseBodyMessage} Notification: {dto.ToJsonLog()}");
                    break;
                }
                catch (Exception ex)
                {
                    httpNotSendedReason = ex.Dump();
                    Logger.Error(ex, $"Exception while sending notification for customer code: {customer.CustomerName}. Notification: {dto.ToJsonLog()}");
                    break;
                }
            }

            return (sentNotificationIds, notSendedDtosQueue.Select(dto => dto.Id).ToList(), httpNotSendedReason);
        }

        private async Task<(List<int> sentNotificationIds, List<int> unsentNotificationIds, string lastExceptionMessage)> ProcessElasticNotifications(QaConfigCustomer customer, CancellationToken token)
        {
            var notificationDtos = GetPendingNotifications(CdcProviderName.Elastic.ToString());
            var sentNotificationIds = new List<int>();

            string httpNotSendedReason = null;
            var notSendedDtosQueue = new Queue<SystemNotificationDto>(notificationDtos);
            while (notSendedDtosQueue.Any() && token.IsCancellationRequested)
            {
                var dto = notSendedDtosQueue.Peek();
                try
                {
                    var responseMessage = PushDataToHttpChannel(dto.Url, dto.Json, customer.CustomerName);
                    var response = await responseMessage.ReceiveString();
                    if (!(await responseMessage).IsSuccessStatusCode)
                    {
                        httpNotSendedReason = response;
                        Logger.Warn($"Http push notification response was failed for customer code: {customer.CustomerName}: {response}");
                        break;
                    }

                    sentNotificationIds.Add(notSendedDtosQueue.Dequeue().Id);
                    Logger.Trace($"Http push notification was pushed successfuly for customer code: {customer.CustomerName}: {response}");
                }
                catch (Exception ex)
                {
                    httpNotSendedReason = ex.Dump();
                    Logger.Error(ex, $"Exception while sending notification for customer code: {customer.CustomerName}. Notification: {dto.ToJsonLog()}");
                    break;
                }
            }

            return (sentNotificationIds, notSendedDtosQueue.Select(dto => dto.Id).ToList(), httpNotSendedReason);
        }

        private IEnumerable<SystemNotificationDto> GetPendingNotifications(string providerName)
        {
            using (new QPConnectionScope())
            {
                var notificationModels = _externalNotificationService.GetPendingNotifications(providerName);
                return Mapper.Map<List<SystemNotificationModel>, List<SystemNotificationDto>>(notificationModels).OrderBy(n => n.TransactionLsn).ToList();
            }
        }

        private void UpdateDbNotificationsData((List<int> sentNotificationIds, List<int> unsentNotificationIds, string lastExceptionMessage) sentUnsentTuple)
        {
            using (new QPConnectionScope())
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
