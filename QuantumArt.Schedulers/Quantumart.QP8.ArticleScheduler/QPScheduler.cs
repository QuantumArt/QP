using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using QP8.Infrastructure.Logging;
using Quantumart.QP8.ArticleScheduler.Interfaces;
using Quantumart.QP8.Configuration.Models;

namespace Quantumart.QP8.ArticleScheduler
{
    public class QpScheduler : IScheduler
    {
        private readonly IUnityContainer _unityContainer;
        private readonly List<QaConfigCustomer> _customers;

        public QpScheduler(IUnityContainer unityContainer, List<QaConfigCustomer> customers)
        {
            _unityContainer = unityContainer;
            _customers = customers;
        }

        public void Run()
        {
            var commonTasksQueueCount = 0;
            var exceptions = new ConcurrentQueue<Exception>();
            Parallel.ForEach(_customers, customer =>
            {
                try
                {
                    var customerDbScheduler = _unityContainer.Resolve<DbScheduler>(
                        new ParameterOverride("customer", customer),
                        new ParameterOverride("connectionString", customer.ConnectionString)
                    );

                    var customerTasksQueueCount2 = customerDbScheduler.GetTasksCountToProcessAtSpecificDateTime(DateTime.Now);
                    customerDbScheduler.Run();
                    var customerTasksQueueCount = customerDbScheduler.GetTasksCountToProcessAtSpecificDateTime(DateTime.Now.AddHours(3));
                    Interlocked.Add(ref commonTasksQueueCount, customerTasksQueueCount);
                }
                catch (Exception ex)
                {
                    Logger.Log.Error($"There was an error on customer code: {customer.CustomerName}", ex);
                    ex.Data.Add("CustomerCode", customer.CustomerName);
                    exceptions.Enqueue(ex);
                }
            });

            PrtgErrorsHandler.LogMessage(_customers, exceptions.ToList(), commonTasksQueueCount);
        }
    }
}
