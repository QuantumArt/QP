using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Quantumart.QP8.ArticleScheduler.Interfaces;
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
        private readonly TimeSpan _tasksQueueCheckShiftTime;
        private readonly IUnityContainer _unityContainer;
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        public QpScheduler(IUnityContainer unityContainer, List<QaConfigCustomer> customers, TimeSpan tasksQueueCheckShiftTime)
        {
            _unityContainer = unityContainer;
            _customers = customers;
            _tasksQueueCheckShiftTime = tasksQueueCheckShiftTime;
        }

        public void Run()
        {
            Parallel.ForEach(_customers, customer =>
            {
                try
                {
                    var customerTasksQueueCount = ProcessCustomer(customer);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "There was an error on customer code: {customerCode}", customer.CustomerName);
                }
            });
        }

        private int ProcessCustomer(QaConfigCustomer customer)
        {
            var customerDbScheduler = _unityContainer.Resolve<DbScheduler>(
                new ParameterOverride("customer", customer),
                new ParameterOverride("connectionString", customer.ConnectionString),
                new ParameterOverride("dbType", customer.DbType)
            );

            QPContext.CurrentCustomerCode = customer.CustomerName;

            Logger.ForTraceEvent()
                .Message("Processing customer code: {customerCode}", customer.CustomerName)
                .Log();

            customerDbScheduler.Run();
            return customerDbScheduler.GetTasksCountToProcessAtSpecificDateTime(DateTime.Now.Add(_tasksQueueCheckShiftTime));
        }
    }
}
