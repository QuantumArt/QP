﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using QP8.Infrastructure.Logging.Interfaces;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Logging;
using Quantumart.QP8.BLL.Services.NotificationSender;
using Quantumart.QP8.Configuration.Models;
using Quantumart.QP8.Scheduler.API;

namespace Quantumart.QP8.Scheduler.Notification.Processors
{
    public class InterfaceCleanupProcessor : IProcessor
    {
        private const int DelayDuration = 100;

        private readonly ILog _logger;
        private readonly PrtgErrorsHandler _prtgLogger;
        private readonly ISchedulerCustomerCollection _schedulerCustomers;
        private readonly IExternalInterfaceNotificationService _externalNotificationService;

        public InterfaceCleanupProcessor(
            ILog logger,
            PrtgErrorsHandler prtgLogger,
            ISchedulerCustomerCollection schedulerCustomers,
            IExternalInterfaceNotificationService externalNotificationService)
        {
            _logger = logger;
            _prtgLogger = prtgLogger;
            _schedulerCustomers = schedulerCustomers;
            _externalNotificationService = externalNotificationService;
        }

        public async Task Run(CancellationToken token)
        {
            _logger.Info("Start cleanup notification queue");
            var items = _schedulerCustomers.GetItems();
            var prtgErrorsHandlerVm = new PrtgErrorsHandlerViewModel(items);
            foreach (var customer in _schedulerCustomers.GetItems())
            {
                try
                {
                    ProcessCustomer(customer);
                    await Task.Delay(DelayDuration, token);
                }
                catch (Exception ex)
                {
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
