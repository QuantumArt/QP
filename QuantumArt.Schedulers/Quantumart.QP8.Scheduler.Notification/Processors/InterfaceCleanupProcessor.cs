using System;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services.NotificationSender;
using Quantumart.QP8.Configuration.Models;
using Quantumart.QP8.Scheduler.API;

namespace Quantumart.QP8.Scheduler.Notification.Processors
{
    public class InterfaceCleanupProcessor : IProcessor
    {
        private const int DelayDuration = 100;
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private readonly ISchedulerCustomerCollection _schedulerCustomers;
        private readonly IExternalInterfaceNotificationService _externalNotificationService;

        public InterfaceCleanupProcessor(
            ISchedulerCustomerCollection schedulerCustomers,
            IExternalInterfaceNotificationService externalNotificationService)
        {
            _schedulerCustomers = schedulerCustomers;
            _externalNotificationService = externalNotificationService;
        }

        public async Task Run(CancellationToken token)
        {
            Logger.Info("Start cleanup notification queue");
            var items = _schedulerCustomers.GetItems();
            foreach (var customer in _schedulerCustomers.GetItems())
            {
                try
                {
                    ProcessCustomer(customer);
                    await Task.Delay(DelayDuration, token);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "There was an error on customer code {customerCode}", customer.CustomerName);
                }
            }

            Logger.Info("End cleanup notification queue");
        }

        private void ProcessCustomer(QaConfigCustomer customer)
        {
            using (new QPConnectionScope(customer.ConnectionString, customer.DbType))
            {
                if (_externalNotificationService.ExistsSentNotifications())
                {
                    Logger.Info("Start cleanup notification queue for customer code {customerCode}", customer.CustomerName);
                    _externalNotificationService.DeleteSentNotifications();
                }
            }
        }
    }
}
