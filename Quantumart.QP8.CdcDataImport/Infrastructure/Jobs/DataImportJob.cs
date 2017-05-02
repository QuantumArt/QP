using System;
using System.Threading.Tasks;
using QP8.Infrastructure.Logging;
using QP8.Infrastructure.Logging.PrtgMonitoring.NLogExtensions.Factories;
using Quantumart.QP8.BLL.Logging;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Constants;
using Quartz;

namespace Quantumart.QP8.CdcDataImport.Infrastructue.Jobs
{
    public class DataImportJob : IJob, IInterruptableJob
    {
        private const string AppName = "QP8CdcDataImportService";
        private readonly PrtgErrorsHandler _prtgLogger;

        public DataImportJob()
        {
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
            Parallel.ForEach(customers, customer =>
            {
                try
                {
                    // CdcImportService.ImportData(CdcImportType.Tarantool);
                    // http://stackoverflow.com/questions/19304300/how-do-i-properly-cancel-parallel-foreach
                    // http://stackoverflow.com/questions/16048260/parallel-foreach-loopstate-stop-versus-cancellation
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

        public void Interrupt()
        {
            Logger.Log.Trace("JOB WAS INTERRUPTED");
        }
    }
}
