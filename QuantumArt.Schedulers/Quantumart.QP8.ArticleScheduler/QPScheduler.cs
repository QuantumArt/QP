using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using QP8.Infrastructure.Logging;
using Quantumart.QP8.ArticleScheduler.Interfaces;
using Quantumart.QP8.BLL.Logging;
using Quantumart.QP8.Configuration.Models;

namespace Quantumart.QP8.ArticleScheduler
{
    public class QpScheduler : IScheduler
    {
        private readonly IUnityContainer _unityContainer;
        private readonly List<QaConfigCustomer> _customers;
        private readonly TimeSpan _tasksQueueCheckShiftTime;
        private readonly PrtgErrorsHandler _prtgLogger;

        public QpScheduler(IUnityContainer unityContainer, List<QaConfigCustomer> customers, TimeSpan tasksQueueCheckShiftTime)
        {
            _unityContainer = unityContainer;
            _customers = customers;
            _tasksQueueCheckShiftTime = tasksQueueCheckShiftTime;
            _prtgLogger = new PrtgErrorsHandler();
        }

        public void Run()
        {
            var prtgErrorsHandlerVm = new PrtgErrorsHandlerViewModel(_customers.ToList());
            Parallel.ForEach(_customers, customer =>
            {
                try
                {
                    var customerDbScheduler = _unityContainer.Resolve<DbScheduler>(
                        new ParameterOverride("customer", customer),
                        new ParameterOverride("connectionString", customer.ConnectionString)
                    );

                    customerDbScheduler.Run();
                    var customerTasksQueueCount = customerDbScheduler.GetTasksCountToProcessAtSpecificDateTime(DateTime.Now.Add(_tasksQueueCheckShiftTime));
                    prtgErrorsHandlerVm.IncrementTasksQueueCount(customerTasksQueueCount);
                }
                catch (Exception ex)
                {
                    ex.Data.Add("CustomerCode", customer.CustomerName);
                    Logger.Log.Error($"There was an error on customer code: {customer.CustomerName}", ex);
                    prtgErrorsHandlerVm.EnqueueNewException(ex);
                }
            });

            _prtgLogger.LogMessage(prtgErrorsHandlerVm);
        }
    }
}