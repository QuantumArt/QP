using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using QP8.Infrastructure.Logging;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Logging;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Configuration.Models;
using Quantumart.QP8.Scheduler.API;

namespace Quantumart.QP8.Scheduler.Notification
{
    public class CleanupProcessor : IProcessor, IDisposable
    {
        private const int DelayDuration = 100;
        private readonly ISchedulerCustomers _schedulerCustomers;
        private readonly IExternalNotificationService _externalNotificationService;
        private readonly PrtgErrorsHandler _prtgLogger;

        public CleanupProcessor(ISchedulerCustomers schedulerCustomers, IExternalNotificationService externalNotificationService)
        {
            _schedulerCustomers = schedulerCustomers;
            _externalNotificationService = externalNotificationService;
            _prtgLogger = new PrtgErrorsHandler();
        }

        public async Task Run(CancellationToken token)
        {
            Logger.Log.Info("Start cleanup notification queue");

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
                    ex.Data.Add("CustomerCode", customer.CustomerName);
                    Logger.Log.Error($"There was an error on customer code: {customer.CustomerName}", ex);
                    prtgErrorsHandlerVm.EnqueueNewException(ex);
                }
            }

            _prtgLogger.LogMessage(prtgErrorsHandlerVm);
            Logger.Log.Info("End cleanup notification queue");
        }

        private void ProcessCustomer(QaConfigCustomer customer)
        {
            using (new QPConnectionScope(customer.ConnectionString))
            {
                if (_externalNotificationService.ExistsSentNotifications())
                {
                    Logger.Log.Info($"Start cleanup notification queue for customer code: {customer.CustomerName}");
                    _externalNotificationService.DeleteSentNotifications();
                }
            }
        }

        public void Dispose()
        {
            Logger.Log.Dispose();
        }
    }
}
