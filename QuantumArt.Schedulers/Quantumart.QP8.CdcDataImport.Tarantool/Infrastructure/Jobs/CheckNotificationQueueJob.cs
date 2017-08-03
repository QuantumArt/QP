using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using QP8.Infrastructure.Logging;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Interfaces.Services;
using Quantumart.QP8.BLL.Logging;
using Quantumart.QP8.BLL.Services.NotificationSender;
using Quantumart.QP8.CdcDataImport.Common.Infrastructure;
using Quantumart.QP8.CdcDataImport.Tarantool.Properties;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Configuration.Models;
using Quartz;

namespace Quantumart.QP8.CdcDataImport.Tarantool.Infrastructure.Jobs
{
    [DisallowConcurrentExecution]
    public class CheckNotificationQueueJob : IInterruptableJob
    {
        private const string AppName = "QP8CdcDataImportService.Tarantool";
        private readonly PrtgErrorsHandler _prtgLogger;
        private readonly IDbService _dbService;
        private readonly IExternalSystemNotificationService _systemNotificationService;
        private readonly CancellationTokenSource _cts;

        public CheckNotificationQueueJob(PrtgErrorsHandler prtgLogger, IDbService dbService, IExternalSystemNotificationService systemNotificationService)
        {
            _prtgLogger = prtgLogger;
            _dbService = dbService;
            _systemNotificationService = systemNotificationService;
            _cts = new CancellationTokenSource();
        }

        public void Execute(IJobExecutionContext context)
        {
            var customers = QPConfiguration.GetCustomers(AppName).Where(c => !(c.ExcludeFromSchedulers || c.ExcludeFromSchedulersCdcTarantool)).ToList();
            var customersDictionary = new Dictionary<QaConfigCustomer, bool>();
            var prtgErrorsHandlerVm = new PrtgErrorsHandlerViewModel(customers);

            var customersWithEnabledCdc = customers.Where(customer =>
            {
                try
                {
                    return ShouldUseCdcForCustomerCode(customer);
                }
                catch (Exception ex)
                {
                    ex.Data.Clear();
                    ex.Data.Add("CustomerCode", customer.CustomerName);
                    Logger.Log.Warn($"There was an error while reading customer code settings: {customer.CustomerName}", ex);
                    prtgErrorsHandlerVm.EnqueueNewException(ex);
                }

                return false;
            }).ToList();

            foreach (var customer in customersWithEnabledCdc)
            {
                try
                {
                    customersDictionary.Add(customer, IsCustomerQueueEmpty(customer));
                }
                catch (Exception ex)
                {
                    ex.Data.Clear();
                    ex.Data.Add("CustomerCode", customer.CustomerName);
                    Logger.Log.Warn($"There was an error while reading customer code settings: {customer.CustomerName}", ex);
                    prtgErrorsHandlerVm.EnqueueNewException(ex);
                }
            }

            CdcSynchronizationContext.ReplaceData(customersDictionary);
            _prtgLogger.LogMessage(prtgErrorsHandlerVm);
        }

        private bool ShouldUseCdcForCustomerCode(QaConfigCustomer customer)
        {
            using (new QPConnectionScope(customer.ConnectionString))
            {
                return _dbService.GetDbSettings().UseCdc;
            }
        }

        private bool IsCustomerQueueEmpty(QaConfigCustomer customer)
        {
            using (new QPConnectionScope(customer.ConnectionString))
            {
                return !_systemNotificationService.ExistsUnsentNotifications(Settings.Default.HttpEndpoint);
            }
        }

        public void Interrupt()
        {
            CdcSynchronizationContext.CustomersDictionary.Clear();
            _cts.Cancel();

            Logger.Log.Trace("CheckNotificationQueueJob was interrupted");
        }
    }
}
