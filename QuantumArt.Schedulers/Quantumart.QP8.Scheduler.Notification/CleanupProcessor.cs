using System;
using System.Threading;
using System.Threading.Tasks;
using QP8.Infrastructure.Logging;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Scheduler.API;

namespace Quantumart.QP8.Scheduler.Notification
{
    public class CleanupProcessor : IProcessor, IDisposable
    {
        private const int DelayDuration = 100;
        private readonly ISchedulerCustomers _schedulerCustomers;
        private readonly IExternalNotificationService _externalNotificationService;

        public CleanupProcessor(ISchedulerCustomers schedulerCustomers, IExternalNotificationService externalNotificationService)
        {
            _schedulerCustomers = schedulerCustomers;
            _externalNotificationService = externalNotificationService;
        }

        public async Task Run(CancellationToken token)
        {
            Logger.Log.Info("Start cleanup notification queue");
            await ProcessCustomers(token);
            Logger.Log.Info("End cleanup notification queue");
        }

        private async Task ProcessCustomers(CancellationToken token)
        {
            foreach (var customer in _schedulerCustomers)
            {
                using (new QPConnectionScope(customer.ConnectionString))
                {
                    if (_externalNotificationService.ExistsSentNotifications())
                    {
                        Logger.Log.Info($"Cleanup notification queue for customer code: {customer.CustomerName}");
                        _externalNotificationService.DeleteSentNotifications();
                    }
                }

                await Task.Delay(DelayDuration, token);
            }
        }

        public void Dispose()
        {
            Logger.Log.Dispose();
        }
    }
}
