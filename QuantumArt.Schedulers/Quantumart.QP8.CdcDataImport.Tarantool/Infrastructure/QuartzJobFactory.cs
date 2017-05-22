using System;
using QP8.Infrastructure.Logging;
using QP8.Infrastructure.Logging.PrtgMonitoring.NLogExtensions.Factories;
using Quantumart.QP8.BLL.Logging;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.BLL.Services.NotificationSender;
using Quantumart.QP8.CdcDataImport.Tarantool.Infrastructure.Jobs;
using Quantumart.QP8.Constants;
using Quartz;
using Quartz.Spi;

namespace Quantumart.QP8.CdcDataImport.Tarantool.Infrastructure
{
    internal class QuartzJobFactory : IJobFactory
    {
        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            // http://www.mpustelak.com/2017/01/topshelf-and-quartz-net-with-dependency-injection/
            if (bundle.JobDetail.JobType == typeof(CdcDataImportJob))
            {
                return new CdcDataImportJob(
                    new PrtgErrorsHandler(
                        new PrtgNLogFactory(
                            LoggerData.DefaultPrtgServiceStateVariableName,
                            LoggerData.DefaultPrtgServiceQueueVariableName,
                            LoggerData.DefaultPrtgServiceStatusVariableName
                        )
                    ),
                    new CdcImportService(),
                    new ExternalSystemNotificationService()
                );
            }

            if (bundle.JobDetail.JobType == typeof(CheckNotificationQueueJob))
            {
                return new CheckNotificationQueueJob(
                    new PrtgErrorsHandler(
                        new PrtgNLogFactory(
                            LoggerData.DefaultPrtgServiceStateVariableName,
                            LoggerData.DefaultPrtgServiceQueueVariableName,
                            LoggerData.DefaultPrtgServiceStatusVariableName
                        )
                    ),
                    new DbService(),
                    new ExternalSystemNotificationService()
                );
            }

            throw new Exception("Not registered Job Type was requested");
        }

        public void ReturnJob(IJob job)
        {
            Logger.Log.Debug("Job was finished");
        }
    }
}
