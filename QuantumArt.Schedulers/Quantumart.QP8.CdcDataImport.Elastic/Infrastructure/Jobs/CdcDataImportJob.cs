using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using QP8.Infrastructure.Logging;
using Quantumart.QP8.BLL.Logging;
using Quantumart.QP8.BLL.Services.NotificationSender;
using Quantumart.QP8.CdcDataImport.Common.Infrastructure;
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
        }

        public void Interrupt()
        {
            _cts.Cancel();
            Logger.Log.Debug("CdcDataImportJob was interrupted");
        }
    }
}
