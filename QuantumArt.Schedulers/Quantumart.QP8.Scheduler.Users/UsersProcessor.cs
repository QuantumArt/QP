using System;
using System.Threading;
using System.Threading.Tasks;
using QP8.Infrastructure.Logging;
using QP8.Infrastructure.Logging.Interfaces;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services.UserSynchronization;
using Quantumart.QP8.Scheduler.API;

namespace Quantumart.QP8.Scheduler.Users
{
    public class UsersProcessor : IProcessor, IDisposable
    {
        private const int DelayDuration = 100;

        private readonly ILog _logger;
        private readonly ISchedulerCustomers _schedulerCustomers;
        private readonly Func<IUserSynchronizationService> _getSynchronizationService;

        public UsersProcessor(ILog logger, ISchedulerCustomers schedulerCustomers, Func<IUserSynchronizationService> getSynchronizationService)
        {
            _logger = logger;
            _schedulerCustomers = schedulerCustomers;
            _getSynchronizationService = getSynchronizationService;
        }

        public async Task Run(CancellationToken token)
        {
            _logger.Info("Start users synchronization");
            foreach (var customer in _schedulerCustomers)
            {
                using (new QPConnectionScope(customer.ConnectionString))
                {
                    var service = _getSynchronizationService();
                    if (service.NeedSynchronization())
                    {
                        _logger.Info($"Start synchronization for customer code: ${customer.CustomerName}");
                        service.Synchronize();
                    }
                }

                await Task.Delay(DelayDuration, token);
            }

            _logger.Info("End users synchronization");
        }

        public void Dispose()
        {
            Logger.Log.Dispose();
        }
    }
}
