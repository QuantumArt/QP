using QP8.Infrastructure.Logging;
using Quartz;

namespace Quantumart.QP8.CdcDataImport.Common.Listeners
{
    public class SchedulerListener : ISchedulerListener
    {
        public void JobScheduled(ITrigger trigger)
        {
            Logger.Log.Trace($"SCHEDULER LISTENER: Job Scheduled from trigger: {trigger.Key.Name}");
        }

        public void JobUnscheduled(TriggerKey triggerKey)
        {
            Logger.Log.Trace($"SCHEDULER LISTENER: Job Unscheduled from trigger: {triggerKey.Name}");
        }

        public void TriggerFinalized(ITrigger trigger)
        {
            Logger.Log.Trace($"SCHEDULER LISTENER: Trigger Finalized from trigger: {trigger.Key.Name}");
        }

        public void TriggerPaused(TriggerKey triggerKey)
        {
            Logger.Log.Trace($"SCHEDULER LISTENER: Trigger Paused from trigger: {triggerKey.Name}");
        }

        public void TriggersPaused(string triggerGroup)
        {
            Logger.Log.Trace($"SCHEDULER LISTENER: Trigger Paused for group: {triggerGroup}");
        }

        public void TriggerResumed(TriggerKey triggerKey)
        {
            Logger.Log.Trace($"SCHEDULER LISTENER: Trigger Paused from trigger: {triggerKey.Name}");
        }

        public void TriggersResumed(string triggerGroup)
        {
            Logger.Log.Trace($"SCHEDULER LISTENER: Trigger Resumed for group: {triggerGroup}");
        }

        public void JobAdded(IJobDetail jobDetail)
        {
            Logger.Log.Trace($"SCHEDULER LISTENER: Job Added: {jobDetail.Key.Name}");
        }

        public void JobDeleted(JobKey jobKey)
        {
            Logger.Log.Trace($"SCHEDULER LISTENER: Job Deleted: {jobKey.Name}");
        }

        public void JobPaused(JobKey jobKey)
        {
            Logger.Log.Trace($"SCHEDULER LISTENER: Job Paused: {jobKey.Name}");
        }

        public void JobsPaused(string jobGroup)
        {
            Logger.Log.Trace($"SCHEDULER LISTENER: Jobs Paused for group: {jobGroup}");
        }

        public void JobResumed(JobKey jobKey)
        {
            Logger.Log.Trace($"SCHEDULER LISTENER: Job Resumed: {jobKey.Name}");
        }

        public void JobsResumed(string jobGroup)
        {
            Logger.Log.Trace($"SCHEDULER LISTENER: Jobs Resumed for group: {jobGroup}");
        }

        public void SchedulerError(string msg, SchedulerException cause)
        {
            Logger.Log.Trace($"SCHEDULER LISTENER: Scheduler error: {msg} with exception: {cause.Message}");
        }

        public void SchedulerInStandbyMode()
        {
            Logger.Log.Trace($"SCHEDULER LISTENER: Scheduler is in standby");
        }

        public void SchedulerStarted()
        {
            Logger.Log.Trace($"SCHEDULER LISTENER: Scheduler started");
        }

        public void SchedulerStarting()
        {
            Logger.Log.Trace($"SCHEDULER LISTENER: Scheduler starting...");
        }

        public void SchedulerShutdown()
        {
            Logger.Log.Trace($"SCHEDULER LISTENER: Scheduler shutdown");
        }

        public void SchedulerShuttingdown()
        {
            Logger.Log.Trace($"SCHEDULER LISTENER: Scheduler shutting down...");
        }

        public void SchedulingDataCleared()
        {
            Logger.Log.Trace($"SCHEDULER LISTENER: Scheduling data cleard");
        }
    }
}
