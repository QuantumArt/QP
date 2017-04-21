using System.Linq;
using QP8.Infrastructure;
using QP8.Infrastructure.Logging.Factories;
using QP8.Infrastructure.Logging.PrtgMonitoring.Data;
using QP8.Infrastructure.Logging.PrtgMonitoring.Interfaces;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.BLL.Logging
{
    public class PrtgErrorsHandler
    {
        private const string SuccessLogMessage = "All tasks successfully proceeded.";
        private const string ErrorLogMessage = "Some schedulers were failed to finsih task processing.";
        private const string CriticalErrorLogMessage = "All schedulers were failed to finish task processing.";

        private readonly IPrtgServiceLogger _prtgLogger;

        public PrtgErrorsHandler()
            : this(LoggerData.DefaultPrtgServiceLoggerName)
        {
        }

        public PrtgErrorsHandler(string loggerName)
        {
            _prtgLogger = (IPrtgServiceLogger)LogProvider.GetLogger(loggerName);
        }

        public void LogMessage(PrtgServiceMonitoringMessage logMessage)
        {
            _prtgLogger.LogMessage(logMessage);
        }

        public void LogMessage(PrtgErrorsHandlerViewModel prtgErrorsHandlerViewModel)
        {
            Ensure.Argument.IsNot(prtgErrorsHandlerViewModel.CustomersExceptions.Count > prtgErrorsHandlerViewModel.Customers.Count, "Exceptions count shouldn't be greater than number of customers");

            if (!prtgErrorsHandlerViewModel.CustomersExceptions.Any())
            {
                _prtgLogger.LogMessage(new PrtgServiceMonitoringMessage
                {
                    Message = SuccessLogMessage,
                    Queue = prtgErrorsHandlerViewModel.CustomersTasksQueueCount,
                    State = PrtgServiceMonitoringEnum.Ok
                });
            }
            else if (prtgErrorsHandlerViewModel.CustomersExceptions.Count < prtgErrorsHandlerViewModel.Customers.Count)
            {
                _prtgLogger.LogMessage(new PrtgServiceMonitoringMessage
                {
                    Message = ErrorLogMessage,
                    Queue = prtgErrorsHandlerViewModel.CustomersTasksQueueCount,
                    State = PrtgServiceMonitoringEnum.Error
                });
            }
            else
            {
                _prtgLogger.LogMessage(new PrtgServiceMonitoringMessage
                {
                    Message = CriticalErrorLogMessage,
                    Queue = prtgErrorsHandlerViewModel.CustomersTasksQueueCount,
                    State = PrtgServiceMonitoringEnum.CriticalError
                });
            }
        }
    }
}
