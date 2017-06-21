using QP8.Infrastructure.Logging;
using Quartz;

namespace Quantumart.QP8.CdcDataImport.Common.Infrastructure.JobListeners
{
    public class SchedulerListener : ISchedulerListener
    {
        private const string LogStartMessage = "SCHEDULER LISTENER:";

        public void JobScheduled(ITrigger trigger)
        {
            Logger.Log.Trace($"{LogStartMessage} Job Scheduled from trigger: {trigger.Key.Name}");
        }

        public void JobUnscheduled(TriggerKey triggerKey)
        {
            Logger.Log.Trace($"{LogStartMessage} Job Unscheduled from trigger: {triggerKey.Name}");
        }

        public void TriggerFinalized(ITrigger trigger)
        {
            Logger.Log.Trace($"{LogStartMessage} Trigger Finalized from trigger: {trigger.Key.Name}");
        }

        public void TriggerPaused(TriggerKey triggerKey)
        {
            Logger.Log.Trace($"{LogStartMessage} Trigger Paused from trigger: {triggerKey.Name}");
        }

        public void TriggersPaused(string triggerGroup)
        {
            Logger.Log.Trace($"{LogStartMessage} Trigger Paused for group: {triggerGroup}");
        }

        public void TriggerResumed(TriggerKey triggerKey)
        {
            Logger.Log.Trace($"{LogStartMessage} Trigger Paused from trigger: {triggerKey.Name}");
        }

        public void TriggersResumed(string triggerGroup)
        {
            Logger.Log.Trace($"{LogStartMessage} Trigger Resumed for group: {triggerGroup}");
        }

        public void JobAdded(IJobDetail jobDetail)
        {
            Logger.Log.Trace($"{LogStartMessage} Job Added: {jobDetail.Key.Name}");
        }

        public void JobDeleted(JobKey jobKey)
        {
            Logger.Log.Trace($"{LogStartMessage} Job Deleted: {jobKey.Name}");
        }

        public void JobPaused(JobKey jobKey)
        {
            Logger.Log.Trace($"{LogStartMessage} Job Paused: {jobKey.Name}");
        }

        public void JobsPaused(string jobGroup)
        {
            Logger.Log.Trace($"{LogStartMessage} Jobs Paused for group: {jobGroup}");
        }

        public void JobResumed(JobKey jobKey)
        {
            Logger.Log.Trace($"{LogStartMessage} Job Resumed: {jobKey.Name}");
        }

        public void JobsResumed(string jobGroup)
        {
            Logger.Log.Trace($"{LogStartMessage} Jobs Resumed for group: {jobGroup}");
        }

        public void SchedulerError(string msg, SchedulerException error)
        {
            Logger.Log.Fatal($"{LogStartMessage} Scheduler error: {msg} with exception", error);
        }

        public void SchedulerInStandbyMode()
        {
            Logger.Log.Trace($"{LogStartMessage} Scheduler is in standby");
        }

        public void SchedulerStarted()
        {
            Logger.Log.Trace($"{LogStartMessage} Scheduler started");
        }

        public void SchedulerStarting()
        {
            Logger.Log.Trace($"{LogStartMessage} Scheduler starting...");
        }

        public void SchedulerShutdown()
        {
            Logger.Log.Trace($"{LogStartMessage} Scheduler shutdown");
        }

        public void SchedulerShuttingdown()
        {
            Logger.Log.Trace($"{LogStartMessage} Scheduler shutting down...");
        }

        public void SchedulingDataCleared()
        {
            Logger.Log.Trace($"{LogStartMessage} Scheduling data clear");
        }
    }
}
