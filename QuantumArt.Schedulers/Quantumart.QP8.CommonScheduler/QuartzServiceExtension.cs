using AutoMapper.Configuration;
using Microsoft.Extensions.Configuration;
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
        public static IServiceCollection AddQuartzService(this IServiceCollection services, string name, IConfigurationSection section)
        {
            services.AddQuartz(conf =>
            {
                conf.UseMicrosoftDependencyInjectionScopedJobFactory();
                conf.SchedulerId = "AUTO";
                if (section != null)
                {
                    RegisterAllTasks(conf, section);
                }
                conf.SchedulerName = name;
            });

            services.AddQuartzHostedService();

            return services;
        }

        public static void RegisterAllTasks(IServiceCollectionQuartzConfigurator configurator, IConfigurationSection rootSection)
        {
            AddTask<EnableUsersJob>(configurator, rootSection);
            AddTask<DisableUsersJob>(configurator, rootSection);
            AddTask<UsersSynchronizationJob>(configurator, rootSection);
            AddTask<InterfaceNotificationJob>(configurator, rootSection);
            AddTask<SystemNotificationJob>(configurator, rootSection);
            AddTask<InterfaceCleanupJob>(configurator, rootSection);
            AddTask<SystemCleanupJob>(configurator, rootSection);
            AddTask<T.CdcDataImportJob>(configurator, rootSection);
            AddTask<T.CheckNotificationQueueJob>(configurator, rootSection);
            AddTask<T.CdcDataImportJob>(configurator, rootSection);
            AddTask<T.CheckNotificationQueueJob>(configurator, rootSection);
        }

        public static void AddTask<TJob>(IServiceCollectionQuartzConfigurator configurator, IConfigurationSection rootSection)
            where TJob : class, IJob
        {
            var jobProperties = QuartzService.GetJobProperties<TJob>(rootSection);
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
