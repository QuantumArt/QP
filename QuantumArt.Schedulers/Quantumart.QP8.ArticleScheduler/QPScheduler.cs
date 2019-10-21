using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Quantumart.QP8.ArticleScheduler.Interfaces;
using Quantumart.QP8.BLL.Logging;
using Quantumart.QP8.Configuration.Models;
using Unity;
using Unity.Resolution;
using NLog;
using NLog.Fluent;
using Quantumart.QP8.BLL;

namespace Quantumart.QP8.ArticleScheduler
{
    public class QpScheduler : IScheduler
    {
        private readonly List<QaConfigCustomer> _customers;
        private readonly PrtgErrorsHandler _prtgLogger;
        private readonly TimeSpan _tasksQueueCheckShiftTime;
        private readonly IUnityContainer _unityContainer;
        private static ILogger Logger = LogManager.GetCurrentClassLogger();

        public QpScheduler(IUnityContainer unityContainer, PrtgErrorsHandler prtgLogger, List<QaConfigCustomer> customers, TimeSpan tasksQueueCheckShiftTime)
        {
            _unityContainer = unityContainer;
            _customers = customers;
            _tasksQueueCheckShiftTime = tasksQueueCheckShiftTime;
            _prtgLogger = prtgLogger;
        }

        public void Run()
        {
            var prtgErrorsHandlerVm = new PrtgErrorsHandlerViewModel(_customers);
            Parallel.ForEach(_customers, customer =>
            {
                try
                {
                    var customerTasksQueueCount = ProcessCustomer(customer);
                    prtgErrorsHandlerVm.IncrementTasksQueueCount(customerTasksQueueCount);
                }
                catch (Exception ex)
                {
                    ex.Data.Clear();
                    ex.Data.Add("CustomerCode", customer.CustomerName);
                    Logger.Error()
                        .Exception(ex)
                        .Message("There was an error on customer code: {customerCode}", customer.CustomerName)
                        .Write();
                    prtgErrorsHandlerVm.EnqueueNewException(ex);
                }
            });

            _prtgLogger.LogMessage(prtgErrorsHandlerVm);
        }

        private int ProcessCustomer(QaConfigCustomer customer)
        {
            var customerDbScheduler = _unityContainer.Resolve<DbScheduler>(
                new ParameterOverride("customer", customer),
                new ParameterOverride("connectionString", customer.ConnectionString),
                new ParameterOverride("dbType", customer.DbType)
            );

            QPContext.CurrentCustomerCode = customer.CustomerName;

            Logger.Trace()
                .Message("Processing customer code: {customerCode}", customer.CustomerName)
                .Write();

            customerDbScheduler.Run();
            return customerDbScheduler.GetTasksCountToProcessAtSpecificDateTime(DateTime.Now.Add(_tasksQueueCheckShiftTime));
        }
    }
}
