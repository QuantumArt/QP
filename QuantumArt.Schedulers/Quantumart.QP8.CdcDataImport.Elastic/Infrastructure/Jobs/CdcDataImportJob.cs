using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using AutoMapper;
using Flurl.Http;
using QP8.Infrastructure;
using QP8.Infrastructure.Logging;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Logging;
using Quantumart.QP8.BLL.Models.NotificationSender;
using Quantumart.QP8.BLL.Services.NotificationSender;
using Quantumart.QP8.CdcDataImport.Common.Infrastructure;
using Quantumart.QP8.CdcDataImport.Elastic.Infrastructure.Data;
using Quantumart.QP8.CdcDataImport.Elastic.Properties;
using Quantumart.QP8.Configuration.Models;
using Quartz;

namespace Quantumart.QP8.CdcDataImport.Elastic.Infrastructure.Jobs
{
    [DisallowConcurrentExecution]
    public class CdcDataImportJob : IInterruptableJob
    {
        private readonly PrtgErrorsHandler _prtgLogger;
        private readonly CancellationTokenSource _cts;
        private readonly CdcImportService _cdcImportService;
        private readonly IExternalSystemNotificationService _systemNotificationService;

        public CdcDataImportJob(PrtgErrorsHandler prtgLogger, CdcImportService cdcImportService, IExternalSystemNotificationService systemNotificationService)
        {
            _prtgLogger = prtgLogger;
            _cts = new CancellationTokenSource();
            _cdcImportService = cdcImportService;
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
                    ProcessCustomer(customerKvp.Key, customerKvp.Value);
                }
                catch (Exception ex)
                {
                    ex.Data.Add("CustomerCode", customerKvp.Key.CustomerName);
                    Logger.Log.Error($"There was an error on customer code: {customerKvp.Key.CustomerName}", ex);
                    prtgErrorsHandlerVm.EnqueueNewException(ex);
                }
            });

            _prtgLogger.LogMessage(prtgErrorsHandlerVm);
            Logger.Log.Trace($"All tasks for thread #{Thread.CurrentThread.ManagedThreadId} were proceeded successfuly");
        }

        private async void ProcessCustomer(QaConfigCustomer customer, bool isNotificationQueueEmpty)
        {
            var isSuccessfulHttpResonse = true;
            var tableTypeModels = GetCdcDataModels(customer.ConnectionString, out string lastExecutedLsn);

            Ensure.Items(tableTypeModels, ttm => ttm.ToLsn == lastExecutedLsn, "All cdc tables should be proceeded at single transaction");

            var dtosQueue = new Queue<CdcDataTableDto>(GroupCdcTableTypeModelsByLsn(customer.CustomerName, tableTypeModels));
            string lastPushedLsn = null;
            if (isNotificationQueueEmpty)
            {
                while (isSuccessfulHttpResonse && dtosQueue.Any() && !_cts.Token.IsCancellationRequested)
                {
                    isSuccessfulHttpResonse = false;
                    var data = dtosQueue.Peek();
                    try
                    {
                        isSuccessfulHttpResonse = await PushDataToHttpChannel(data);
                    }
                    catch (FlurlHttpException ex)
                    {
                        Logger.Log.Warn("There was an error while sending http push notification", ex);
                    }

                    if (isSuccessfulHttpResonse)
                    {
                        lastPushedLsn = dtosQueue.Dequeue().TransactionLsn;
                    }
                }
            }

            AddDataToNotificationQueue(customer, dtosQueue.ToList(), lastPushedLsn, lastExecutedLsn);
            if (!isSuccessfulHttpResonse)
            {
                CdcSynchronizationContext.SetCustomerNotificationQueueNotEmpty(customer);
            }
        }

        private List<CdcTableTypeModel> GetCdcDataModels(string connectionString, out string toLsn)
        {
            List<CdcTableTypeModel> result;
            using (var ts = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted }))
            using (new QPConnectionScope(connectionString))
            {
                var fromLsn = _cdcImportService.GetLastExecutedLsn();
                toLsn = _cdcImportService.GetMaxLsn();
                result = _cdcImportService.GetCdcDataFromTables(fromLsn, toLsn);
                ts.Complete();
            }

            return result;
        }

        private static IEnumerable<CdcDataTableDto> GroupCdcTableTypeModelsByLsn(string customerName, IEnumerable<CdcTableTypeModel> tableTypeModels)
        {
            return tableTypeModels
                .GroupBy(gr => gr.TransactionLsn)
                .OrderBy(gr => gr.Key).ToList()
                .Select(gr => new CdcDataTableDto
                {
                    CustomerCode = customerName,
                    TransactionLsn = gr.Key,
                    TransactionDate = gr.Select(data => data.TransactionDate).First(),
                    Changes = gr.OrderBy(data => data.SequenceLsn).Select((data, i) => new CdcChangeDto
                    {
                        Action = data.Action,
                        ChangeType = data.ChangeType,
                        OrderNumber = i,
                        Entity = Mapper.Map<CdcEntityModel, CdcEntityDto>(data.Entity)
                    }).ToList()
                }).ToList();
        }

        private static async Task<bool> PushDataToHttpChannel(CdcDataTableDto data) => (
            await Settings.Default.HttpEndpoint
                .AllowAnyHttpStatus()
                .WithTimeout(Settings.Default.HttpTimeout)
                .PostJsonAsync(data)
            ).IsSuccessStatusCode;

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
