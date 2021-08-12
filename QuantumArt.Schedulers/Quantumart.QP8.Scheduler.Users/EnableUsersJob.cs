using System;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services.UserSynchronization;
using Quantumart.QP8.Configuration.Models;
using Quantumart.QP8.Scheduler.API;
using Quartz;

namespace Quantumart.QP8.Scheduler.Users
{
    [DisallowConcurrentExecution]
    [PersistJobDataAfterExecution]
    public class EnableUsersJob : IJob
    {
        private const string ExcludedUsersKey = "ExcludedUsers";

        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private readonly ISchedulerCustomerCollection _schedulerCustomers;
        private readonly ICommonUserService _commonUserService;

        public EnableUsersJob(
            ISchedulerCustomerCollection schedulerCustomers,
            ICommonUserService commonUserService
        )
        {
            _schedulerCustomers = schedulerCustomers;
            _commonUserService = commonUserService;
        }

        public Task Execute(IJobExecutionContext context)
        {
            Logger.Info($"Start enabling");
            var customers = _schedulerCustomers.GetItems();

            var dataMap = context.MergedJobDataMap;
            var excludedUsers = dataMap.GetString(ExcludedUsersKey)?.Split(',');

            foreach (var customer in customers)
            {
                try
                {
                    if (context.CancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    if (!context.CancellationToken.IsCancellationRequested)
                    {
                        ProcessCustomer(customer, excludedUsers, context.CancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    ex.Data.Clear();
                    ex.Data.Add("CustomerCode", customer.CustomerName);
                    Logger.Error(ex, $"There was an error on customer code: {customer.CustomerName}", ex);
                }
            }

            Logger.Info("Finished enabling users");
            return Task.CompletedTask;
        }

        private void ProcessCustomer(QaConfigCustomer customer, string[] excludedUsers, CancellationToken token)
        {
            using (new QPConnectionScope(customer.ConnectionString, customer.DbType))
            {
                _commonUserService.EnableUsers(customer.CustomerName, excludedUsers, token);
            }
        }
    }
}
