using System;
using Quantumart.QP8.CdcDataImport.Common.Infrastructure.JobListeners;
using Quartz;
using Quartz.Impl.Matchers;
using Topshelf.Quartz;

namespace Quantumart.QP8.CdcDataImport.Common
{
    public class JobConfigurator
    {
        public static void GetConfigurationForJob<T>(QuartzConfigurator configurator, TimeSpan recurrentTimeout)
            where T : IJob
        {
            IJobDetail JobBuilder() => Quartz.JobBuilder.Create<T>().Build();
            ITrigger TriggerBuilder() => Quartz.TriggerBuilder.Create().WithSimpleSchedule(b => b.WithInterval(recurrentTimeout).RepeatForever()).Build();
            QuartzJobListenerConfig JobListenerFn() => new QuartzJobListenerConfig(new JobListener(), KeyMatcher<JobKey>.KeyEquals(new JobKey("cdcJob", "cdcGroup")));
            QuartzTriggerListenerConfig TriggerLoggerFn() => new QuartzTriggerListenerConfig(new TriggerListener(), KeyMatcher<TriggerKey>.KeyEquals(new TriggerKey("cdcJob", "cdcGroup")));
            ISchedulerListener SchedulerLoggerFn() => new SchedulerListener();

            configurator
                .WithJob(JobBuilder)
                .AddTrigger(TriggerBuilder)
                .WithJobListener(JobListenerFn)
                .WithTriggerListener(TriggerLoggerFn)
                .WithScheduleListener(SchedulerLoggerFn);
        }
    }
}
