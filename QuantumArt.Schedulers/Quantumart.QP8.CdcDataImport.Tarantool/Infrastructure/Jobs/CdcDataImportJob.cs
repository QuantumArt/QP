using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using AutoMapper;
using Flurl.Http;
using QP8.Infrastructure;
using QP8.Infrastructure.Extensions;
using QP8.Infrastructure.Logging;
using QP8.Infrastructure.Web.Enums;
using QP8.Infrastructure.Web.Responses;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Logging;
using Quantumart.QP8.BLL.Models.NotificationSender;
using Quantumart.QP8.BLL.Services.NotificationSender;
using Quantumart.QP8.CdcDataImport.Common.Infrastructure;
using Quantumart.QP8.CdcDataImport.Tarantool.Infrastructure.Data;
using Quantumart.QP8.CdcDataImport.Tarantool.Infrastructure.Services;
using Quantumart.QP8.CdcDataImport.Tarantool.Properties;
using Quantumart.QP8.Configuration.Models;
using Quantumart.QP8.Constants.Cdc;
using Quartz;

namespace Quantumart.QP8.CdcDataImport.Tarantool.Infrastructure.Jobs
{
    [DisallowConcurrentExecution]
    public class CdcDataImportJob : IInterruptableJob
    {
        private readonly PrtgErrorsHandler _prtgLogger;
        private readonly CancellationTokenSource _cts;
        private readonly ICdcImportService _cdcImportService;
        private readonly ICdcDataImportProcessor _cdcImportProcessor;
        private readonly IExternalSystemNotificationService _systemNotificationService;

        public CdcDataImportJob(PrtgErrorsHandler prtgLogger, ICdcImportService cdcImportService, ICdcDataImportProcessor cdcImportProcessor, IExternalSystemNotificationService systemNotificationService)
        {
            _prtgLogger = prtgLogger;
            _cts = new CancellationTokenSource();
            _cdcImportService = cdcImportService;
            _cdcImportProcessor = cdcImportProcessor;
            _systemNotificationService = systemNotificationService;
        }

        public void Execute(IJobExecutionContext context)
        {
            var customersDictionary = CdcSynchronizationContext.CustomersDictionary;
            var prtgErrorsHandlerVm = new PrtgErrorsHandlerViewModel(customersDictionary.Select(kvp => kvp.Key).ToList());
            var po = new ParallelOptions
            {
                CancellationToken = _cts.Token,
                MaxDegreeOfParallelism = Environment.ProcessorCount
            };

            Parallel.ForEach(customersDictionary, po, (customerKvp, loopState) =>
            {
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
                    Logger.Log.Warn($"There was an error on customer code: {customerKvp.Key.CustomerName}", ex);
                    prtgErrorsHandlerVm.EnqueueNewException(ex);
                }
            });

            _prtgLogger.LogMessage(prtgErrorsHandlerVm);
            Logger.Log.Trace($"All tasks for thread #{Thread.CurrentThread.ManagedThreadId} were proceeded successfuly");
        }

        internal async Task ProcessCustomer(QaConfigCustomer customer, bool isNotificationQueueEmpty)
        {
            var shouldSendHttpRequests = isNotificationQueueEmpty;
            var tableTypeModels = GetCdcDataModels(customer.ConnectionString, out string lastExecutedLsn);

            Ensure.Items(tableTypeModels, ttm => ttm.ToLsn == lastExecutedLsn, "All cdc tables should be proceeded at single transaction");

            var notSendedDtosQueue = new Queue<CdcDataTableDto>(GroupCdcTableTypeModelsByLsn(customer.CustomerName, tableTypeModels));
            string lastPushedLsn = null;
            while (shouldSendHttpRequests && notSendedDtosQueue.Any() && !_cts.Token.IsCancellationRequested)
            {
                shouldSendHttpRequests = false;
                var data = notSendedDtosQueue.Peek();
                try
                {
                    var responseMessage = PushDataToHttpChannel(data);
                    (await responseMessage).EnsureSuccessStatusCode();

                    var response = await responseMessage.ReceiveJson<JSendResponse>();
                    if (response.Status == JSendStatus.Success && response.Code == 200)
                    {
                        shouldSendHttpRequests = true;
                        lastPushedLsn = notSendedDtosQueue.Dequeue().TransactionLsn;
                        Logger.Log.Trace($"Http push notification was pushed successfuly: {response.ToJsonLog()}");
                    }
                    else
                    {
                        Logger.Log.Warn($"Http push notification response was failed: {response.ToJsonLog()}");
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log.Warn("There was an http error while sending http push notification", ex);
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

        private List<CdcTableTypeModel> GetCdcDataModels(string connectionString, out string toLsn)
        {
            List<CdcTableTypeModel> result;
            using (var ts = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted }))
            using (new QPConnectionScope(connectionString))
            {
                var fromLsn = _cdcImportService.GetLastExecutedLsn();
                toLsn = _cdcImportService.GetMaxLsn();
                result = _cdcImportProcessor.GetCdcDataFromTables(fromLsn, toLsn);
                ts.Complete();
            }

            return result;
        }

        private static async Task<HttpResponseMessage> PushDataToHttpChannel(CdcDataTableDto data) =>
            await Settings.Default.HttpEndpoint.AllowAnyHttpStatus().WithTimeout(Settings.Default.HttpTimeout).PostJsonAsync(new NginxResponse(data));

        private void AddDataToNotificationQueue(QaConfigCustomer customer, List<CdcDataTableDto> data, string lastPushedLsn, string lastExecutedLsn)
        {
            using (var ts = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted }))
            using (new QPConnectionScope(customer.ConnectionString))
            {
                var notificationsData = Mapper.Map<List<CdcDataTableDto>, List<SystemNotificationModel>>(data);
                _systemNotificationService.InsertNotification(notificationsData);
                _cdcImportService.PostLastExecutedLsn(Settings.Default.HttpEndpoint, lastPushedLsn, lastExecutedLsn);
                ts.Complete();
            }
        }

        public void Interrupt()
        {
            _cts.Cancel();
            Logger.Log.Debug("CdcDataImportJob was interrupted");
        }
    }
}
