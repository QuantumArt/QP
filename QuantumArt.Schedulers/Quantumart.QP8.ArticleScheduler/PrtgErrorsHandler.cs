using System;
using System.Collections.Generic;
using System.Linq;
using QP8.Infrastructure;
using QP8.Infrastructure.Logging.Factories;
using QP8.Infrastructure.Logging.PrtgMonitoring.NLogExtensions;
using QP8.Infrastructure.Logging.PrtgMonitoring.NLogExtensions.Factories;
using QP8.Infrastructure.Logging.PrtgMonitoring.NLogExtensions.Interfaces;
using Quantumart.QP8.Configuration.Models;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.ArticleScheduler
{
    internal class PrtgErrorsHandler
    {
        private static readonly IPrtgServiceLogger PrtgLogger;

        static PrtgErrorsHandler()
        {
            PrtgLogger = (IPrtgServiceLogger)LogProvider.GetLogger(LoggerData.DefaultPrtgServiceLoggerName);
        }

        internal static void LogMessage(List<QaConfigCustomer> customers, List<Exception> listOfExceptions, int commonTasksQueueCount = 0)
        {
            Ensure.Argument.IsNot(listOfExceptions.Count > customers.Count, "Exceptions count shouldn't be greater than number of customers");

            if (!listOfExceptions.Any())
            {
                PrtgLogger.LogMessage(new PrtgServiceMonitoringMessage
                {
                    Message = "All tasks successfully proceeded.",
                    Queue = commonTasksQueueCount,
                    State = PrtgServiceMonitoringEnum.Ok
                });
            }
            else if (listOfExceptions.Count < customers.Count)
            {
                PrtgLogger.LogMessage(new PrtgServiceMonitoringMessage
                {
                    Message = "Some schedulers were failed to finsih task processing.",
                    Queue = commonTasksQueueCount,
                    State = PrtgServiceMonitoringEnum.Error
                });
            }
            else
            {
                PrtgLogger.LogMessage(new PrtgServiceMonitoringMessage
                {
                    Message = "All schedulers were failed to finish task processing.",
                    Queue = commonTasksQueueCount,
                    State = PrtgServiceMonitoringEnum.CriticalError
                });
            }
        }
    }
}
