using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using QP8.Infrastructure.Logging;
using QP8.Infrastructure.Logging.PrtgMonitoring.NLogExtensions.Factories;
using QP8.Infrastructure.Logging.PrtgMonitoring.NLogExtensions.Interfaces;
using Quantumart.QP8.ArticleScheduler.Interfaces;
using Quantumart.QP8.BLL.Logging;
using Quantumart.QP8.Configuration.Models;
using Quantumart.QP8.Constants;

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

            // TODO: fix
            _prtgLogger = new PrtgErrorsHandler(new PrtgNLogFactory(
                LoggerData.DefaultPrtgServiceStateVariableName,
                LoggerData.DefaultPrtgServiceQueueVariableName,
                LoggerData.DefaultPrtgServiceStatusVariableName
            ));

            _prtgLogger = new PrtgErrorsHandler(_unityContainer.Resolve<IPrtgNLogFactory>());
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
                    Logger.Log.Error($"There was an error on customer code: {customer.CustomerName}", ex);
                    prtgErrorsHandlerVm.EnqueueNewException(ex);
                }
            });

            _prtgLogger.LogMessage(prtgErrorsHandlerVm);
        }

        private int ProcessCustomer(QaConfigCustomer customer)
        {
            var customerDbScheduler = _unityContainer.Resolve<DbScheduler>(
                new ParameterOverride("customer", customer),
                new ParameterOverride("connectionString", customer.ConnectionString)
            );

            customerDbScheduler.Run();
            return customerDbScheduler.GetTasksCountToProcessAtSpecificDateTime(DateTime.Now.Add(_tasksQueueCheckShiftTime));
        }
    }
}
