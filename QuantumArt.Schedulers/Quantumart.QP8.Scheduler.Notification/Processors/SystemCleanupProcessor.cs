using QP8.Infrastructure.Logging.Interfaces;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Logging;
using Quantumart.QP8.BLL.Services.NotificationSender;
using Quantumart.QP8.Configuration.Models;
using Quantumart.QP8.Scheduler.API;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Quantumart.QP8.Scheduler.Notification.Processors
{
    public class SystemCleanupProcessor : IProcessor
    {
        private const int DelayDuration = 100;

        private readonly ILog _logger;
        private readonly PrtgErrorsHandler _prtgLogger;
        private readonly ISchedulerCustomers _schedulerCustomers;
        private readonly IExternalNotificationService _externalNotificationService;

        public SystemCleanupProcessor(
            ILog logger,
            PrtgErrorsHandler prtgLogger,
            ISchedulerCustomers schedulerCustomers,
            IExternalNotificationService externalNotificationService)
        {
            _logger = logger;
            _prtgLogger = prtgLogger;
            _schedulerCustomers = schedulerCustomers;
            _externalNotificationService = externalNotificationService;
        }

        public async Task Run(CancellationToken token)
        {
            _logger.Info("Start cleanup notification queue");
            var prtgErrorsHandlerVm = new PrtgErrorsHandlerViewModel(_schedulerCustomers.ToList());
            foreach (var customer in _schedulerCustomers)
            {
                try
                {
                    ProcessCustomer(customer);
                    await Task.Delay(DelayDuration, token);
                }
                catch (Exception ex)
                {
                    ex.Data.Clear();
                    ex.Data.Add("CustomerCode", customer.CustomerName);
                    _logger.Error($"There was an error on customer code: {customer.CustomerName}", ex);
                    prtgErrorsHandlerVm.EnqueueNewException(ex);
                }
            }

            _prtgLogger.LogMessage(prtgErrorsHandlerVm);
            _logger.Info("End cleanup notification queue");
        }

        private void ProcessCustomer(QaConfigCustomer customer)
        {
            using (new QPConnectionScope(customer.ConnectionString))
            {
                if (_externalNotificationService.ExistsSentNotifications())
                {
                    _logger.Info($"Start cleanup notification queue for customer code: {customer.CustomerName}");
                    _externalNotificationService.DeleteSentNotifications();
                }
            }
        }
    }
}
