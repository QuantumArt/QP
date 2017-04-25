using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using QP8.Infrastructure.Logging.Interfaces;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Logging;
using Quantumart.QP8.BLL.Services.UserSynchronization;
using Quantumart.QP8.Configuration.Models;
using Quantumart.QP8.Scheduler.API;

namespace Quantumart.QP8.Scheduler.Users
{
    public class UsersProcessor : IProcessor
    {
        private const int DelayDuration = 100;

        private readonly ILog _logger;
        private readonly PrtgErrorsHandler _prtgLogger;
        private readonly ISchedulerCustomers _schedulerCustomers;
        private readonly Func<IUserSynchronizationService> _getSynchronizationService;

        public UsersProcessor(
            ILog logger,
            PrtgErrorsHandler prtgLogger,
            ISchedulerCustomers schedulerCustomers,
            Func<IUserSynchronizationService> getSynchronizationService)
        {
            _logger = logger;
            _prtgLogger = prtgLogger;
            _schedulerCustomers = schedulerCustomers;
            _getSynchronizationService = getSynchronizationService;
        }

        public async Task Run(CancellationToken token)
        {
            _logger.Info("Start users synchronization");
            var prtgErrorsHandlerVm = new PrtgErrorsHandlerViewModel(_schedulerCustomers.ToList());
            foreach (var customer in _schedulerCustomers)
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
                    ex.Data.Add("CustomerCode", customer.CustomerName);
                    _logger.Error($"There was an error on customer code: {customer.CustomerName}", ex);
                    prtgErrorsHandlerVm.EnqueueNewException(ex);
                }

                await Task.Delay(DelayDuration, token);
            }

            _prtgLogger.LogMessage(prtgErrorsHandlerVm);
            _logger.Info("End users synchronization");
        }

        private void ProcessCustomer(QaConfigCustomer customer)
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
        }
    }
}
