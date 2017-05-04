using QP8.Infrastructure.Logging;
using Quartz;

namespace Quantumart.QP8.CdcDataImport.Common.Listeners
{
    public class JobListener : IJobListener
    {
        private readonly string _jobListenerName;

        public JobListener()
            : this("Job Listener")
        {
        }

        public JobListener(string jobListenerName)
        {
            _jobListenerName = jobListenerName;
        }

        public string Name
        {
            get { return _jobListenerName; }
        }

        public void JobToBeExecuted(IJobExecutionContext context)
        {
            Logger.Log.Trace($"JOB LISTENER: Job is about to execute: {context.JobDetail.Key.Name}");
        }

        public void JobExecutionVetoed(IJobExecutionContext context)
        {
            Logger.Log.Trace($"JOB LISTENER: Job execution vetoed: {context.JobDetail.Key.Name}");
        }

        public void JobWasExecuted(IJobExecutionContext context, JobExecutionException jobException)
        {
            Logger.Log.Trace($"JOB LISTENER: Job was executed: {context.JobDetail.Key.Name}");
        }
    }
}
