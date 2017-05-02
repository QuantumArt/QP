using System;
using QP8.Infrastructure.Logging;
using Quantumart.QP8.CdcDataImport.Infrastructue.Jobs;
using Quantumart.QP8.CdcDataImport.Infrastructure.Logging;
using Quantumart.QP8.CdcDataImport.Properties;
using Quartz;
using Quartz.Impl.Matchers;
using Topshelf;
using Topshelf.Quartz;

namespace Quantumart.QP8.CdcDataImport
{
    public class ServiceHost : IServiceHost
    {
        public bool Start(HostControl hostControl)
        {
            // http://stackoverflow.com/questions/25930384/how-do-i-force-a-quartz-net-job-to-restart-intervall-after-completion
            Func<IJobDetail> jobBuilder = () => JobBuilder.Create<DataImportJob>().Build();
            Func<ITrigger> triggerBuilder = () => TriggerBuilder.Create().WithSimpleSchedule(b => b.WithInterval(Settings.Default.RecurrentTimeout).RepeatForever()).Build();
            Func<QuartzJobListenerConfig> jobListenerFn = () => new QuartzJobListenerConfig(new JobListener(), KeyMatcher<JobKey>.KeyEquals(new JobKey("job1", "group1")));
            Func<QuartzTriggerListenerConfig> triggerLoggerFn = () => new QuartzTriggerListenerConfig(new TriggerListener(), KeyMatcher<TriggerKey>.KeyEquals(new TriggerKey("job1", "group1")));
            Func<ISchedulerListener> schedulerLoggerFn = () => new SchedulerListener();

            sc.ScheduleQuartzJob(q => q
                .WithJob(jobBuilder)
                .AddTrigger(triggerBuilder)
                .WithJobListener(jobListenerFn)
                .WithTriggerListener(triggerLoggerFn)
                .WithScheduleListener(schedulerLoggerFn)
            );

            Logger.Log.Trace("Service host was started");
            return true;
        }

        public bool Stop()
        {
            Logger.Log.Trace("Service host has stopped");
            return true;
        }

        public bool Pause()
        {
            Logger.Log.Trace("Service host has paused");
            return true;
        }

        public bool Continue()
        {
            Logger.Log.Trace("Service host has continued");
            return true;
        }
    }
}
