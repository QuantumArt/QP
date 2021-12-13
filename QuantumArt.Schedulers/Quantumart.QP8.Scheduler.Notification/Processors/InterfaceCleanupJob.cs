using System;
using System.Threading.Tasks;
using NLog;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services.NotificationSender;
using Quantumart.QP8.CommonScheduler;
using Quantumart.QP8.Configuration.Models;
using Quantumart.QP8.Scheduler.API;
using Quartz;

namespace Quantumart.QP8.Scheduler.Notification.Processors
{
    public class InterfaceCleanupJob : IJob
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private readonly ISchedulerCustomerCollection _schedulerCustomers;
        private readonly IExternalInterfaceNotificationService _externalNotificationService;

        public InterfaceCleanupJob(
            ISchedulerCustomerCollection schedulerCustomers,
            IExternalInterfaceNotificationService externalNotificationService)
        {
            _schedulerCustomers = schedulerCustomers;
            _externalNotificationService = externalNotificationService;
        }

        public Task Execute(IJobExecutionContext context)
        {
            Logger.Info("Start cleanup notification queue");
            var items = _schedulerCustomers.GetItems();
            items = JobHelpers.FilterCustomers(items, context.MergedJobDataMap);
            foreach (var customer in items)
            {
                try
                {
                    ProcessCustomer(customer);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "There was an error on customer code {customerCode}", customer.CustomerName);
                }
            }

            Logger.Info("End cleanup notification queue");
            return Task.CompletedTask;
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
