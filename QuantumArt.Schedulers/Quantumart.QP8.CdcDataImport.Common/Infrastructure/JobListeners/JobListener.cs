using QP8.Infrastructure.Logging;
using Quartz;

namespace Quantumart.QP8.CdcDataImport.Common.Infrastructure.JobListeners
{
    public class JobListener : IJobListener
    {
        private const string LogStartMessage = "JOB LISTENER:";

        public JobListener()
            : this("Job Listener")
        {
        }

        public JobListener(string jobListenerName)
        {
            Name = jobListenerName;
        }

        public string Name { get; }

        public void JobToBeExecuted(IJobExecutionContext context)
        {
            Logger.Log.Trace($"{LogStartMessage} Job is about to execute: {context.JobDetail.Key.Name}");
        }

        public void JobExecutionVetoed(IJobExecutionContext context)
        {
            Logger.Log.Trace($"{LogStartMessage} Job execution vetoed: {context.JobDetail.Key.Name}");
        }

        public void JobWasExecuted(IJobExecutionContext context, JobExecutionException jobException)
        {
            Logger.Log.Trace($"{LogStartMessage} Job was executed: {context.JobDetail.Key.Name}");
        }
    }
}
