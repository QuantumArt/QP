using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using AutoMapper;
using Flurl.Http;
using Newtonsoft.Json;
using QP8.Infrastructure;
using QP8.Infrastructure.Extensions;
using NLog;
using QP8.Infrastructure.Web.Enums;
using QP8.Infrastructure.Web.Responses;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Models.NotificationSender;
using Quantumart.QP8.BLL.Services.CdcImport;
using Quantumart.QP8.BLL.Services.NotificationSender;
using Quantumart.QP8.CdcDataImport.Common.Infrastructure;
using Quantumart.QP8.CdcDataImport.Tarantool.Infrastructure.Data;
using Quantumart.QP8.Configuration.Models;
using Quantumart.QP8.Constants.Cdc.Enums;
using Quantumart.QP8.Constants.Cdc.Tarantool;
using Quartz;

namespace Quantumart.QP8.CdcDataImport.Tarantool.Infrastructure.Jobs
{
    public class CdcDataImportJob : IJob
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        private readonly CancellationTokenSource _cts;
        private readonly ICdcImportService _cdcImportService;
        private readonly IExternalSystemNotificationService _systemNotificationService;

        public CdcDataImportJob(
            ICdcImportService cdcImportService,
            IExternalSystemNotificationService systemNotificationService
        )
        {
            _cdcImportService = cdcImportService;
            _systemNotificationService = systemNotificationService;
        }

        public Task Execute(IJobExecutionContext context)
        {
            var customersDictionary = CdcSynchronizationContext.CustomersDictionary;
            var token = context.CancellationToken;
            var po = new ParallelOptions
            {
                CancellationToken = token,
                MaxDegreeOfParallelism = Environment.ProcessorCount
            };

            Parallel.ForEach(customersDictionary, po, (customerKvp, loopState) =>
            {
                if (token.IsCancellationRequested)
                {
                    loopState.Break();
                }
                if (loopState.ShouldExitCurrentIteration || loopState.IsExceptional)
                {
                    loopState.Stop();
                }

                try
                {
                    ProcessCustomer(customerKvp.Key, customerKvp.Value).Wait();
                }
                catch (Exception ex)
                {
                    ex.Data.Add("CustomerCode", customerKvp.Key.CustomerName);
                    Logger.Warn(ex, $"There was an error on customer code: {customerKvp.Key.CustomerName}");
                }
            });

            Logger.Trace($"All tasks for thread #{Thread.CurrentThread.ManagedThreadId} were proceeded successfuly");
            return token.IsCancellationRequested ? Task.FromCanceled(token) : Task.CompletedTask;
        }

        internal async Task ProcessCustomer(QaConfigCustomer customer, bool isNotificationQueueEmpty)
        {
            var shouldSendHttpRequests = isNotificationQueueEmpty;
            var tableTypeModels = GetCdcDataModels(customer, out var lastExecutedLsn);

            Ensure.Items(tableTypeModels, ttm => ttm.ToLsn == lastExecutedLsn, "All cdc tables should be proceeded at single transaction");

            string lastPushedLsn = null;
            var notSendedDtosQueue = new Queue<CdcDataTableDto>(GroupCdcTableTypeModelsByLsn(customer.CustomerName, tableTypeModels));
            while (shouldSendHttpRequests && notSendedDtosQueue.Any() && !_cts.Token.IsCancellationRequested)
            {
                shouldSendHttpRequests = false;
                var data = notSendedDtosQueue.Peek();
                Task<HttpResponseMessage> responseMessage = null;

                try
                {
                    responseMessage = PushDataToHttpChannel(data);
                    var response = await responseMessage.ReceiveJson<JSendResponse>();
                    if (response.Status != JSendStatus.Success || response.Code != 200)
                    {
                        Logger.Warn($"Http push notification response was failed for customer code: {customer.CustomerName} [{data.TransactionLsn}]: {response.ToJsonLog()}");
                        break;
                    }

                    shouldSendHttpRequests = true;
                    lastPushedLsn = notSendedDtosQueue.Dequeue().TransactionLsn;
                    Logger.Trace($"Http push notification was pushed successfuly for customer code: {customer.CustomerName} [{data.TransactionLsn}]: {response.ToJsonLog()}");
                }
                catch (Exception ex) when (ex is JsonReaderException)
                {
                    var responseBodyMessage = $"Response body: {await responseMessage.ReceiveString()}.";
                    Logger.Warn(ex, $"Exception while parsing response for customer code: {customer.CustomerName}. {responseBodyMessage} Notification: {data.ToJsonLog()}");
                    break;
                }
                catch (Exception ex)
                {
                    Logger.Warn(ex, $"There was an http exception while sending push notification for customer code: {customer.CustomerName}. Notification: {data.ToJsonLog()}");
                    break;
                }
            }

            AddDataToNotificationQueue(customer, notSendedDtosQueue.ToList(), lastPushedLsn, lastExecutedLsn);
            if (!shouldSendHttpRequests)
            {
                Ensure.That(notSendedDtosQueue.Any() || !isNotificationQueueEmpty);
                if (isNotificationQueueEmpty)
                {
                    CdcSynchronizationContext.SetCustomerNotificationQueueNotEmpty(customer);
                }
            }
        }

        private static IEnumerable<CdcDataTableDto> GroupCdcTableTypeModelsByLsn(string customerName, IEnumerable<CdcTableTypeModel> tableTypeModels)
        {
            return tableTypeModels
                .AsParallel()
                .GroupBy(gr => gr.TransactionLsn)
                .Select(gr => new CdcDataTableDto
                {
                    CustomerCode = customerName,
                    TransactionLsn = gr.Key,
                    TransactionDate = gr.Select(data => data.TransactionDate).First(),
                    Changes = gr.OrderBy(data => data.SequenceLsn).Select((data, i) => new CdcChangeDto
                    {
                        Action = data.Action.ToString().ToLower(),
                        ChangeType = data.ChangeType.ToString().ToLower(),
                        SequenceLsn = data.SequenceLsn,
                        OrderNumber = i,
                        Entity = data.Entity.EntityType == TarantoolContentArticleModel.EntityType
                            ? Mapper.Map<CdcEntityModel, CdcArticleEntityDto>(data.Entity)
                            : Mapper.Map<CdcEntityModel, CdcEntityDto>(data.Entity)
                    }).ToList()
                }).OrderBy(dto => dto.TransactionLsn).ToList();
        }

        private List<CdcTableTypeModel> GetCdcDataModels(QaConfigCustomer customer, out string toLsn)
        {
            List<CdcTableTypeModel> result;
            using (var ts = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted }))
            using (new QPConnectionScope(customer.ConnectionString, customer.DbType))
            {
                var fromLsn = _cdcImportService.GetLastExecutedLsn(Settings.Default.HttpEndpoint);
                toLsn = _cdcImportService.GetMaxLsn();
                result = _cdcImportService.GetCdcDataFromTables(fromLsn, toLsn);
                ts.Complete();
            }

            return result;
        }

        private void AddDataToNotificationQueue(QaConfigCustomer customer, IEnumerable<CdcDataTableDto> data, string lastPushedLsn, string lastExecutedLsn)
        {
            Ensure.That(lastPushedLsn == null || !string.IsNullOrWhiteSpace(lastPushedLsn));
            using (var ts = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted }))
            using (new QPConnectionScope(customer.ConnectionString, customer.DbType))
            {
                var cdcLastExecutedLsnId = _cdcImportService.PostLastExecutedLsn(CdcProviderName.Tarantool.ToString(), Settings.Default.HttpEndpoint, lastPushedLsn, lastExecutedLsn);
                _systemNotificationService.InsertNotification(GetSystemNotificationModels(cdcLastExecutedLsnId, data));
                ts.Complete();
            }
        }

        private static IEnumerable<SystemNotificationModel> GetSystemNotificationModels(int cdcLastExecutedLsnId, IEnumerable<CdcDataTableDto> cdcDataTableDtos)
        {
            foreach (var dto in cdcDataTableDtos)
            {
                var systemNotificationModel = Mapper.Map<CdcDataTableDto, SystemNotificationModel>(dto);
                systemNotificationModel.CdcLastExecutedLsnId = cdcLastExecutedLsnId;
                yield return systemNotificationModel;
            }
        }

        private static async Task<HttpResponseMessage> PushDataToHttpChannel(CdcDataTableDto data) =>
            await Settings.Default.HttpEndpoint.Replace("{customercode}", data.CustomerCode).AllowAnyHttpStatus().WithTimeout(Settings.Default.HttpTimeout).PostJsonAsync(new NginxRequest(data));

        public void Interrupt()
        {
            _cts.Cancel();
            Logger.Debug("CdcDataImportJob was interrupted");
        }
    }
}
