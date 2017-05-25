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
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Logging;
using Quantumart.QP8.BLL.Models.NotificationSender;
using Quantumart.QP8.BLL.Services.NotificationSender;
using Quantumart.QP8.CdcDataImport.Common.Infrastructure;
using Quantumart.QP8.CdcDataImport.Common.Infrastructure.Extensions;
using Quantumart.QP8.CdcDataImport.Tarantool.Infrastructure.Data;
using Quantumart.QP8.CdcDataImport.Tarantool.Properties;
using Quantumart.QP8.Configuration.Models;
using Quantumart.QP8.Constants;
using Quartz;

namespace Quantumart.QP8.CdcDataImport.Tarantool.Infrastructure.Jobs
{
    [DisallowConcurrentExecution]
    public class CdcDataImportJob : IInterruptableJob
    {
        private readonly PrtgErrorsHandler _prtgLogger;
        private readonly CancellationTokenSource _cts;
        private readonly ICdcImportService _cdcImportService;
        private readonly IExternalSystemNotificationService _systemNotificationService;

        public CdcDataImportJob(PrtgErrorsHandler prtgLogger, ICdcImportService cdcImportService, IExternalSystemNotificationService systemNotificationService)
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
            var isSuccessfulHttpResponse = true;
            var tableTypeModels = GetCdcDataModels(customer.ConnectionString, out string lastExecutedLsn);

            Ensure.Items(tableTypeModels, ttm => ttm.ToLsn == lastExecutedLsn, "All cdc tables should be proceeded at single transaction");

            var dtosQueue = new Queue<CdcDataTableDto>(GroupCdcTableTypeModelsByLsn(customer.CustomerName, tableTypeModels));
            string lastPushedLsn = null;
            if (isNotificationQueueEmpty)
            {
                while (isSuccessfulHttpResponse && dtosQueue.Any() && !_cts.Token.IsCancellationRequested)
                {
                    isSuccessfulHttpResponse = false;
                    var data = dtosQueue.Peek();
                    try
                    {
                        var responseMessage = await PushDataToHttpChannel(data);
                        isSuccessfulHttpResponse = responseMessage.IsSuccessStatusCode;
                        Logger.Log.Trace($"Http push notification was pushed with response status code: {responseMessage.StatusCode}");
                    }
                    catch (FlurlHttpException ex)
                    {
                        Logger.Log.Warn("There was an error while sending http push notification", ex);
                    }

                    if (isSuccessfulHttpResponse)
                    {
                        lastPushedLsn = dtosQueue.Dequeue().TransactionLsn;
                    }
                }
            }

            AddDataToNotificationQueue(customer, dtosQueue.ToList(), lastPushedLsn, lastExecutedLsn);
            if (!isSuccessfulHttpResponse)
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
                result = GetCdcDataFromTables(fromLsn, toLsn);
                ts.Complete();
            }

            return result;
        }

        public List<CdcTableTypeModel> GetCdcDataFromTables(string fromLsn = null, string toLsn = null)
        {
            Logger.Log.Trace($"Getting cdc data for with fromLsn:{fromLsn} and toLsn:{toLsn}");
            var tablesGroup = new List<List<CdcTableTypeModel>>
            {
                _cdcImportService.ImportData(CdcCaptureConstants.Content, fromLsn, toLsn),
                _cdcImportService.ImportData(CdcCaptureConstants.ContentAttribute, fromLsn, toLsn),
                _cdcImportService.ImportData(CdcCaptureConstants.ContentToContent, fromLsn, toLsn),
                _cdcImportService.ImportData(CdcCaptureConstants.ContentToContentRev, fromLsn, toLsn),
                _cdcImportService.ImportData(CdcCaptureConstants.StatusType, fromLsn, toLsn),
                _cdcImportService.ImportData(CdcCaptureConstants.ItemToItem, fromLsn, toLsn),
                _cdcImportService.ImportData(CdcCaptureConstants.ItemLinkAsync, fromLsn, toLsn),
                GetContentArticleDtoFilteredByNetChanges(fromLsn, toLsn).ToList()
            };

            return tablesGroup.SelectMany(tg => tg).OrderBy(t => t.TransactionLsn).ToList();
        }

        private IEnumerable<CdcTableTypeModel> GetContentArticleDtoFilteredByNetChanges(string fromLsn = null, string toLsn = null)
        {
            var contentItemModel = _cdcImportService.ImportData(CdcCaptureConstants.ContentItem, fromLsn, toLsn);
            var contentDataModel = _cdcImportService.ImportData(CdcCaptureConstants.ContentData, fromLsn, toLsn);

            var contentItemsFilteredByNetChanges = contentItemModel.FilterCdcTableTypeByLsnNetChanges(cim => new
            {
                id = (string)cim.Entity.Columns["CONTENT_ITEM_ID"],
                cim.TransactionLsn
            });

            var contentDataFilteredByNetChanges = contentDataModel.FilterCdcTableTypeByLsnNetChanges(cdm => new
            {
                id = (string)cdm.Entity.Columns["CONTENT_ITEM_ID"],
                cdm.TransactionLsn
            });

            foreach (var cim in contentItemsFilteredByNetChanges)
            {
                var fieldColumns = contentDataFilteredByNetChanges
                    .OrderBy(cdm => cdm.SequenceLsn)
                    .Select(cdt => new KeyValuePair<string, object>($"field_{cdt.Entity.Columns["ATTRIBUTE_ID"]}", cdt.Entity.Columns["DATA"]));

                cim.Entity.Columns.AddRange(fieldColumns);
                yield return cim;
            }
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
                        Action = data.Action.ToLowerInvariant(),
                        ChangeType = data.ChangeType.ToString().ToLowerInvariant(),
                        OrderNumber = i,
                        Entity = Mapper.Map<CdcEntityModel, CdcEntityDto>(data.Entity)
                    }).ToList()
                }).ToList();
        }

        private static async Task<HttpResponseMessage> PushDataToHttpChannel(CdcDataTableDto data) =>
            await Settings.Default.HttpEndpoint.AllowAnyHttpStatus().WithTimeout(Settings.Default.HttpTimeout).PostJsonAsync(data);

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
