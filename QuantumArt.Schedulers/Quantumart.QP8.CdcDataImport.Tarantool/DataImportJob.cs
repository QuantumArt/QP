using QP8.Infrastructure.Logging;
using QP8.Infrastructure.Logging.PrtgMonitoring.NLogExtensions.Factories;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Enums;
using Quantumart.QP8.BLL.Logging;
using Quantumart.QP8.BLL.Services.CdcDataImport;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Configuration.Models;
using Quantumart.QP8.Constants;
using Quartz;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Quantumart.QP8.CdcDataImport.Tarantool
{
    public class DataImportJob : IJob, IInterruptableJob
    {
        private const string AppName = "QP8CdcDataImportService";
        private readonly PrtgErrorsHandler _prtgLogger;
        private readonly CancellationTokenSource _cts;
        private readonly CdcImportService cdcImportService;

        public DataImportJob()
        {
            _cts = new CancellationTokenSource();
            _cdcImportService = new CdcImportService();
            _prtgLogger = new PrtgErrorsHandler(new PrtgNLogFactory(
                LoggerData.DefaultPrtgServiceStateVariableName,
                LoggerData.DefaultPrtgServiceQueueVariableName,
                LoggerData.DefaultPrtgServiceStatusVariableName
            ));
        }

        public async void Execute(IJobExecutionContext context)
        {
            var message = $"The current time is: {DateTime.Now}";
            Logger.Log.Info(message);
            Console.WriteLine(message);
            await Task.Delay(new TimeSpan(0, 0, 5));

            var customers = QPConfiguration.GetCustomers(AppName, true);
            var prtgErrorsHandlerVm = new PrtgErrorsHandlerViewModel(customers);

            var po = new ParallelOptions()
            {
                CancellationToken = _cts.Token,
                MaxDegreeOfParallelism = Environment.ProcessorCount
            };

            Parallel.ForEach(customers, po, (customer, loopState) =>
            {
                if (po.CancellationToken.IsCancellationRequested)
                {
                    loopState.Stop();
                }

                try
                {
                    ProcessCustomer(customer);
                }
                catch (Exception ex)
                {
                    ex.Data.Add("CustomerCode", customer.CustomerName);
                    Logger.Log.Error($"There was an error on customer code: {customer.CustomerName}", ex);
                    prtgErrorsHandlerVm.EnqueueNewException(ex);
                }
            });

            _prtgLogger.LogMessage(prtgErrorsHandlerVm);
        }

        private void ProcessCustomer(QaConfigCustomer customer)
        {
            using (new QPConnectionScope(customer.ConnectionString))
            {
                _cdcImportService.ImportData(CdcImportType.Tarantool);
            }
        }

        public void Interrupt()
        {
            Logger.Log.Trace("JOB WAS INTERRUPTED");
        }
    }
}
