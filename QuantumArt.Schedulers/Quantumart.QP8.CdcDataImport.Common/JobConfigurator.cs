using Quantumart.QP8.CdcDataImport.Common.Listeners;
using Quartz;
using Quartz.Impl.Matchers;
using System;
using Topshelf.Quartz;

namespace Quantumart.QP8.CdcDataImport.Common.Tarantool.Infrastructure
{
    public class JobConfigurator
    {
        public static void GetConfigurationForJob<T>(QuartzConfigurator configurator, TimeSpan recurrentTimeout)
            where T : IJob
        {
            Func<IJobDetail> jobBuilder = () => JobBuilder.Create<T>().Build();
            Func<ITrigger> triggerBuilder = () => TriggerBuilder.Create().WithSimpleSchedule(b => b.WithInterval(recurrentTimeout).RepeatForever()).Build();
            Func<QuartzJobListenerConfig> jobListenerFn = () => new QuartzJobListenerConfig(new JobListener(), KeyMatcher<JobKey>.KeyEquals(new JobKey("job1", "group1")));
            Func<QuartzTriggerListenerConfig> triggerLoggerFn = () => new QuartzTriggerListenerConfig(new TriggerListener(), KeyMatcher<TriggerKey>.KeyEquals(new TriggerKey("job1", "group1")));
            Func<ISchedulerListener> schedulerLoggerFn = () => new SchedulerListener();

            configurator
                .WithJob(jobBuilder)
                .AddTrigger(triggerBuilder)
                .WithJobListener(jobListenerFn)
                .WithTriggerListener(triggerLoggerFn)
                .WithScheduleListener(schedulerLoggerFn);
        }
    }
}
