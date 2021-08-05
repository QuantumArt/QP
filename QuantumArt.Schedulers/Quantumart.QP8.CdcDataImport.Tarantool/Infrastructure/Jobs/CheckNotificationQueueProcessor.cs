using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services.DbServices;
using Quantumart.QP8.BLL.Services.NotificationSender;
using Quantumart.QP8.CdcDataImport.Common.Infrastructure;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Configuration.Models;
using Quantumart.QP8.Scheduler.API;
using Quartz;

namespace Quantumart.QP8.CdcDataImport.Tarantool.Infrastructure.Jobs
{
    public class CheckNotificationQueueJob : IJob
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        private const string AppName = "QP8CdcDataImportService.Tarantool";
        private readonly IDbService _dbService;
        private readonly IExternalSystemNotificationService _systemNotificationService;

        public CheckNotificationQueueJob(IDbService dbService, IExternalSystemNotificationService systemNotificationService)
        {
            _dbService = dbService;
            _systemNotificationService = systemNotificationService;
        }

        public Task Execute(IJobExecutionContext context)
        {
            var customers = QPConfiguration.GetCustomers(AppName).Where(c => !(c.ExcludeFromSchedulers || c.ExcludeFromSchedulersCdcTarantool)).ToList();
            var customersDictionary = new Dictionary<QaConfigCustomer, bool>();

            var customersWithEnabledCdc = customers.Where(customer =>
            {
                try
                {
                    return ShouldUseCdcForCustomerCode(customer);
                }
                catch (Exception ex)
                {
                    Logger.Warn(ex,"There was an error while reading customer code: {customerCode}", customer.CustomerName);
                }

                return false;
            }).ToList();

            foreach (var customer in customersWithEnabledCdc)
            {
                if (context.CancellationToken.IsCancellationRequested)
                {
                    return Task.FromCanceled(context.CancellationToken);
                }

                try
                {
                    customersDictionary.Add(customer, IsCustomerQueueEmpty(customer));
                }
                catch (Exception ex)
                {
                    Logger.Warn(ex,"There was an error while processing customer code: {customerCode}", customer.CustomerName);
                }
            }

            CdcSynchronizationContext.ReplaceData(customersDictionary);
            return Task.CompletedTask;
        }

        private bool ShouldUseCdcForCustomerCode(QaConfigCustomer customer)
        {
            using (new QPConnectionScope(customer.ConnectionString, customer.DbType))
            {
                return _dbService.GetDbSettings().UseCdc;
            }
        }

        private bool IsCustomerQueueEmpty(QaConfigCustomer customer)
        {
            using (new QPConnectionScope(customer.ConnectionString, customer.DbType))
            {
                return !_systemNotificationService.ExistsUnsentNotifications(Settings.Default.HttpEndpoint);
            }
        }
    }
}
