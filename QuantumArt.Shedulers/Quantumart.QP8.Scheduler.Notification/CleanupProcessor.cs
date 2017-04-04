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
        private readonly IShedulerCustomers _shedulerCustomers;
        private readonly IExternalNotificationService _externalNotificationService;

        public CleanupProcessor(IShedulerCustomers shedulerCustomers, IExternalNotificationService externalNotificationService)
        {
            _shedulerCustomers = shedulerCustomers;
            _externalNotificationService = externalNotificationService;
        }

        public async Task Run(CancellationToken token)
        {
            Logger.Log.Info("Start cleanup notification queue");
            foreach (var customer in _shedulerCustomers)
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

            Logger.Log.Info("End cleanup notification queue");
        }

        public void Dispose()
        {
            Logger.Log.Dispose();
        }
    }
}
