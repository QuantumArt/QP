using System;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services.UserSynchronization;
using Quantumart.QP8.CommonScheduler;
using Quantumart.QP8.Configuration.Models;
using Quantumart.QP8.Scheduler.API;
using Quartz;

namespace Quantumart.QP8.Scheduler.Users
{
    [DisallowConcurrentExecution]
    [PersistJobDataAfterExecution]
    public class DisableUsersJob : IJob
    {
        private const string InactivePeriodKey = "InactivePeriodInDays";

        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private readonly ISchedulerCustomerCollection _schedulerCustomers;
        private readonly ICommonUserService _commonUserService;

        public DisableUsersJob(
            ISchedulerCustomerCollection schedulerCustomers,
            ICommonUserService commonUserService
        )
        {
            _schedulerCustomers = schedulerCustomers;
            _commonUserService = commonUserService;
        }

        public Task Execute(IJobExecutionContext context)
        {
            Logger.Info($"Start disabling inactive users");
            var customers = _schedulerCustomers.GetItems();

            var dataMap = context.MergedJobDataMap;
            customers = JobHelpers.FilterCustomers(customers, dataMap);
            var diff = dataMap.GetIntValue(InactivePeriodKey);

            if (diff <= 0)
            {
                Logger.Warn($"Task is cancelled because inactive period are {diff} days");
                return Task.CompletedTask;
            }

            foreach (var customer in customers)
            {
                try
                {
                    if (!context.CancellationToken.IsCancellationRequested)
                    {
                        ProcessCustomer(customer, diff, context.CancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    ex.Data.Clear();
                    ex.Data.Add("CustomerCode", customer.CustomerName);
                    Logger.Error(ex, $"There was an error on customer code: {customer.CustomerName}", ex);
                }
            }

            Logger.Info($"Finished disabling inactive users");
            return Task.CompletedTask;
        }

        private void ProcessCustomer(QaConfigCustomer customer, int diff, CancellationToken token)
        {
            using (new QPConnectionScope(customer.ConnectionString, customer.DbType))
            {
                _commonUserService.DisableUsers(customer.CustomerName, diff, token);
            }
        }
    }
}
