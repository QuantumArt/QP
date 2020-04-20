using System;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services.UserSynchronization;
using Quantumart.QP8.Configuration.Models;
using Quantumart.QP8.Scheduler.API;

namespace Quantumart.QP8.Scheduler.Users
{
    public class UsersProcessor : IProcessor
    {
        private const int DelayDuration = 100;

        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private readonly ISchedulerCustomerCollection _schedulerCustomers;
        private readonly IUserSynchronizationService _synchronizationService;

        public UsersProcessor(
            ISchedulerCustomerCollection schedulerCustomers,
            UserSynchronizationService synchronizationService)
        {
            _schedulerCustomers = schedulerCustomers;
            _synchronizationService = synchronizationService;
        }

        public async Task Run(CancellationToken token)
        {
            Logger.Info("Start users synchronization");
            var items = _schedulerCustomers.GetItems();
            foreach (var customer in items)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }

                try
                {
                    ProcessCustomer(customer);
                }
                catch (Exception ex)
                {
                    Logger.Warn(ex,"There was an error while processing customer code: {customerCode}", customer.CustomerName);
                }

                await Task.Delay(DelayDuration, token);
            }

            Logger.Info("End users synchronization");
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
