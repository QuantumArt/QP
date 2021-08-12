using System;
using System.Threading.Tasks;
using NLog;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services.UserSynchronization;
using Quantumart.QP8.Configuration.Models;
using Quantumart.QP8.Scheduler.API;
using Quartz;

namespace Quantumart.QP8.Scheduler.Users
{
    public class UsersSynchronizationJob : IJob
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private readonly ISchedulerCustomerCollection _schedulerCustomers;
        private readonly IUserSynchronizationService _synchronizationService;

        public UsersSynchronizationJob(
            ISchedulerCustomerCollection schedulerCustomers,
            IUserSynchronizationService synchronizationService)
        {
            _schedulerCustomers = schedulerCustomers;
            _synchronizationService = synchronizationService;
        }

        public Task Execute(IJobExecutionContext context)
        {
            Logger.Info("Start users synchronization");
            var items = _schedulerCustomers.GetItems();
            foreach (var customer in items)
            {
                try
                {
                    ProcessCustomer(customer);
                }
                catch (Exception ex)
                {
                    Logger.Warn(ex,"There was an error while processing customer code: {customerCode}", customer.CustomerName);
                }

            }

            Logger.Info("End users synchronization");
            return Task.CompletedTask;
        }

        private void ProcessCustomer(QaConfigCustomer customer)
        {
            using (new QPConnectionScope(customer.ConnectionString, customer.DbType))
            {
                if (_synchronizationService.NeedSynchronization())
                {
                    Logger.Info($"Start synchronization for customer code: ${customer.CustomerName}");
                    _synchronizationService.Synchronize();
                }
            }
        }
    }
}
