using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using QP8.Infrastructure.Web.Enums;
using Quantumart.QP8.BLL.Services.API;
using Quantumart.QP8.Scheduler.API.Models;
using Quartz;
using Quartz.Impl.Matchers;

namespace Quantumart.QP8.CommonScheduler
{
    public class QuartzService
    {
        private ILogger Logger { get; set; }
        private IScheduler Scheduler { get; set; }
        private ISchedulerFactory _factory { get; set; }
        private Task<IReadOnlyList<IScheduler>> AllSchedulersTask { get; }
        private string SchedulerName { get; set; }

        public QuartzService(
            ILogger<QuartzService> logger,
            ISchedulerFactory factory,
            CommonSchedulerProperties quartzServiceSettings)
        {
            Logger = logger;
            _factory = factory;
            AllSchedulersTask = factory.GetAllSchedulers();
            SchedulerName = quartzServiceSettings.Name;
        }

        internal static CommonSchedulerTaskProperties GetJobProperties<T>(CommonSchedulerTaskProperties[] tasks) where T : IJob
        {
            var jobName = typeof(T).Name;
            var jobProperties = tasks.FirstOrDefault(f => f.Name.Equals(jobName, StringComparison.InvariantCultureIgnoreCase));
            return jobProperties;
        }

        public async Task<JobStatus> InterruptJob(string key)
        {
            var result = new JobStatus();
            string message;

            try
            {
                Scheduler = await _factory.GetScheduler(SchedulerName);
                if (Scheduler == null)
                {
                    message = $"Stopping job {key}: there is no scheduler with name {SchedulerName}";
                    Logger.Log(LogLevel.Information, message);
                }
                else
                {
                    var jobKeys = await Scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup());
                    var jobKey = jobKeys.FirstOrDefault(jk => jk.Name == key);
                    if (jobKey != null)
                    {
                        Logger.Log(LogLevel.Information, $"Job {key} was forced stopped");
                        await Scheduler.Interrupt(jobKey);

                        result.Status = JSendStatus.Success;
                        result.Message = $"The job {key} was stopped successfully";
                        return result;
                    }

                    message = $"There is no job with identity {key}";
                }
            }
            catch (Exception ex)
            {
                message = $"There was an error while stopping job {key}";
                Logger.LogError(ex, message);
            }

            result.Status = JSendStatus.Fail;
            result.Message = message;
            return result;
        }

        public async Task<List<JobInfo>> GetAllTasks()
        {
            var jobList = new List<JobInfo>();
            if (string.IsNullOrEmpty(SchedulerName))
            {
                return jobList;
            }
            Scheduler = await _factory.GetScheduler(SchedulerName);
            if (Scheduler != null)
            {
                var tasks = await Scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup());
                foreach (var task in tasks)
                {
                    var detail = await Scheduler.GetJobDetail(task);
                    var triggers = await Scheduler.GetTriggersOfJob(task);

                    jobList.Add(new JobInfo
                    {
                        Name = task.Name,
                        Description = detail?.Description,
                        Trigger = ProcessTriggers(triggers)
                    });
                }
            }

            return jobList;
        }


        private List<TriggerInfo> ProcessTriggers(IReadOnlyCollection<ITrigger> triggers)
        {
            var list = new List<TriggerInfo>();
            foreach (var trigger in triggers)
            {
                list.Add(new TriggerInfo
                {
                    Name = trigger.Key.Name,
                    LastStartTime = trigger.GetPreviousFireTimeUtc()?.ToString(),
                    NextStartTime = trigger.GetNextFireTimeUtc()?.ToString(),
                    Schedule = (trigger is ICronTrigger cronTrigger) ? cronTrigger.CronExpressionString : null
                });
            }

            return list;
        }

        public async Task<JobStatus> RunJob(string key)
        {
            var result = new JobStatus();
            string message;
            try
            {
                Scheduler = await _factory.GetScheduler(SchedulerName);
                if (Scheduler == null)
                {
                    message = $"Starting job {key}: there is no scheduler with name {SchedulerName}";
                    Logger.Log(LogLevel.Information, message);
                }
                else
                {
                    var jobKeys = await Scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup());
                    var jobKey = jobKeys.FirstOrDefault(jk => jk.Name == key);
                    if (jobKey != null)
                    {
                        Logger.Log(LogLevel.Information, $"Job {key} was forced running");
                        await Scheduler.TriggerJob(jobKey);

                        result.Status = JSendStatus.Success;
                        result.Message = $"The job {key} was running successfully";
                        return result;
                    }

                    message = $"There is no job with name {key}";
                    Logger.Log(LogLevel.Information, message);
                }
            }
            catch (Exception ex)
            {
                message = $"There was an error while running job {key}";
                Logger.LogError(ex, message);
            }

            result.Status = JSendStatus.Fail;
            result.Message = message;
            return result;
        }

        internal static string GetJobDescription(Type jobType, string description)
        {
            return string.IsNullOrEmpty(description) ? jobType.Name : description;
        }

        internal static string GetJobTriggerIdentity(Type jobType)
        {
            return $"{jobType.Name}_trigger";
        }

        internal static string GetJobTriggerDescription(Type jobType, string cron)
        {
            return $"{jobType.Name}_{cron}";
        }
    }
}
