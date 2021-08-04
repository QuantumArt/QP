using Microsoft.Extensions.DependencyInjection;
using Quantumart.QP8.BLL.Services.API;
using Quantumart.QP8.Scheduler.Notification.Processors;
using Quantumart.QP8.Scheduler.Users;
using Quartz;

using T = Quantumart.QP8.CdcDataImport.Tarantool.Infrastructure.Jobs;
using E = Quantumart.QP8.CdcDataImport.Elastic.Infrastructure.Jobs;

namespace Quantumart.QP8.CommonScheduler
{
    public static class QuartzServiceExtension
    {
        public static IServiceCollection AddQuartzService(this IServiceCollection services, CommonSchedulerProperties schedulerSettings)
        {
            services.AddQuartz(conf =>
            {
                conf.UseMicrosoftDependencyInjectionScopedJobFactory();
                conf.SchedulerId = "AUTO";

                RegisterAllTasks(conf, schedulerSettings.Tasks);
                conf.SchedulerName = schedulerSettings.Name;
            });

            services.AddQuartzHostedService();

            return services;
        }

        public static void RegisterAllTasks(IServiceCollectionQuartzConfigurator configurator, CommonSchedulerTaskProperties[] tasks)
        {
            AddTask<EnableUsersJob>(configurator, tasks);
            AddTask<DisableUsersJob>(configurator, tasks);
            AddTask<UsersSynchronizationJob>(configurator, tasks);
            AddTask<InterfaceNotificationJob>(configurator, tasks);
            AddTask<SystemNotificationJob>(configurator, tasks);
            AddTask<InterfaceCleanupJob>(configurator, tasks);
            AddTask<SystemCleanupJob>(configurator, tasks);
            AddTask<T.CdcDataImportJob>(configurator, tasks);
            AddTask<T.CheckNotificationQueueJob>(configurator, tasks);
            AddTask<T.CdcDataImportJob>(configurator, tasks);
            AddTask<T.CheckNotificationQueueJob>(configurator, tasks);
        }

        public static void AddTask<TJob>(IServiceCollectionQuartzConfigurator configurator, CommonSchedulerTaskProperties[] tasks)
            where TJob : class, IJob
        {
            var jobProperties = QuartzService.GetJobProperties<TJob>(tasks);
            if (jobProperties != null)
            {
                configurator.RegisterJob<TJob>(jobProperties);
            }
        }

        public static IServiceCollectionQuartzConfigurator RegisterJob<TJob>(this IServiceCollectionQuartzConfigurator configurator, CommonSchedulerTaskProperties taskSettings)
            where TJob : class, IJob
        {
            var jobType = typeof(TJob);

            configurator.ScheduleJob<TJob>(
                trigger =>
                    trigger.WithIdentity(QuartzService.GetJobTriggerIdentity(jobType))
                        .WithDescription(QuartzService.GetJobTriggerDescription(jobType, taskSettings.Schedule))
                        .WithCronSchedule(taskSettings.Schedule),
                j =>
                {
                    j
                        .WithIdentity(jobType.Name)
                        .WithDescription(QuartzService.GetJobDescription(jobType, taskSettings.Description));

                    if (taskSettings.SpecifiedConditions != null)
                    {
                        j.UsingJobData(new JobDataMap(taskSettings.SpecifiedConditions));
                    }
                });

            return configurator;
        }
    }
}
