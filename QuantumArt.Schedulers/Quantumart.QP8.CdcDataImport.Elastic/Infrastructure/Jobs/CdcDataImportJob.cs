using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using AutoMapper;
using Flurl;
using Flurl.Http;
using QP8.Infrastructure;
using QP8.Infrastructure.Extensions;
using QP8.Infrastructure.Logging;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Logging;
using Quantumart.QP8.BLL.Models.NotificationSender;
using Quantumart.QP8.BLL.Services.CdcImport;
using Quantumart.QP8.BLL.Services.NotificationSender;
using Quantumart.QP8.CdcDataImport.Common.Infrastructure;
using Quantumart.QP8.CdcDataImport.Elastic.Infrastructure.Data;
using Quantumart.QP8.CdcDataImport.Elastic.Infrastructure.Services;
using Quantumart.QP8.Configuration.Models;
using Quantumart.QP8.Constants.Cdc.Enums;
using Quartz;

namespace Quantumart.QP8.CdcDataImport.Elastic.Infrastructure.Jobs
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
            var tableTypeModels = GetCdcDataModels(customer.ConnectionString, out var lastExecutedLsn).ToList();

            Ensure.Items(tableTypeModels, ttm => ttm.ToLsn == lastExecutedLsn, "All cdc tables should be proceeded at single transaction");

            string lastPushedLsn = null;
            var notSendedDtosQueue = new Queue<CdcDataTableDto>(GetCdcDataTableDtos(customer.CustomerName, tableTypeModels));
            while (shouldSendHttpRequests && notSendedDtosQueue.Any() && !_cts.Token.IsCancellationRequested)
            {
                shouldSendHttpRequests = false;
                var data = notSendedDtosQueue.Peek();
                try
                {
                    var responseMessage = PushDataToHttpChannel(data);
                    var response = await responseMessage.ReceiveString();
                    if (!(await responseMessage).IsSuccessStatusCode)
                    {
                        Logger.Log.Warn($"Http push notification response was failed for customer code: {customer.CustomerName} [{data.TransactionLsn}]: {response}");
                        break;
                    }

                    shouldSendHttpRequests = true;
                    lastPushedLsn = notSendedDtosQueue.Dequeue().TransactionLsn;
                    Logger.Log.Trace($"Http push notification was pushed successfuly for customer code: {customer.CustomerName} [{data.TransactionLsn}]: {response}");
                }
                catch (Exception ex)
                {
                    Logger.Log.Warn($"There was an http exception while sending http push notification for customer code: {customer.CustomerName}. Notification: {data.ToJsonLog()}", ex);
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

        private IEnumerable<CdcTableTypeModel> GetCdcDataModels(string connectionString, out string toLsn)
        {
            List<CdcTableTypeModel> result;
            using (var ts = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted }))
            using (new QPConnectionScope(connectionString))
            {
                var fromLsn = _cdcImportService.GetLastExecutedLsn(Settings.Default.HttpEndpoint);
                toLsn = _cdcImportService.GetMaxLsn();
                result = _cdcImportProcessor.GetCdcDataFromTables(fromLsn, toLsn);
                ts.Complete();
            }

            return result;
        }

        private void AddDataToNotificationQueue(QaConfigCustomer customer, IEnumerable<CdcDataTableDto> data, string lastPushedLsn, string lastExecutedLsn)
        {
            Ensure.That(lastPushedLsn == null || !string.IsNullOrWhiteSpace(lastPushedLsn));
            using (var ts = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted }))
            using (new QPConnectionScope(customer.ConnectionString))
            {
                var cdcLastExecutedLsnId = _cdcImportService.PostLastExecutedLsn(CdcProviderName.Elastic.ToString(), Settings.Default.HttpEndpoint, lastPushedLsn, lastExecutedLsn);
                _systemNotificationService.InsertNotification(GetSystemNotificationModels(cdcLastExecutedLsnId, data));
                ts.Complete();
            }
        }

        private static IEnumerable<CdcDataTableDto> GetCdcDataTableDtos(string customerCode, IEnumerable<CdcTableTypeModel> dataTypeModels)
        {
            foreach (var model in dataTypeModels)
            {
                var cdcDataTable = Mapper.Map<CdcTableTypeModel, CdcDataTableDto>(model);
                cdcDataTable.CustomerCode = customerCode;
                yield return cdcDataTable;
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
            await GetHttpEndpoint(data.CustomerCode).AllowAnyHttpStatus().WithTimeout(Settings.Default.HttpTimeout).PostJsonAsync(data.Entity.Columns);

        private static string GetHttpEndpoint(string customerCode) => Url.Combine(Settings.Default.HttpEndpoint, customerCode, "contentData");

        public void Interrupt()
        {
            _cts.Cancel();
            Logger.Log.Debug("CdcDataImportJob was interrupted");
        }
    }
}
