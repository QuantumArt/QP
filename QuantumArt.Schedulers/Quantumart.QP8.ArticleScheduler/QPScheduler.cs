using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using QP8.Infrastructure.Logging;
using QP8.Infrastructure.Logging.Factories;
using QP8.Infrastructure.Logging.Interfaces;
using Quantumart.QP8.Configuration.Models;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.ArticleScheduler
{
    public class QpScheduler
    {
        private readonly IUnityContainer _unityContainer;
        private readonly List<QaConfigCustomer> _customers;
        private readonly ILog _prtgLogger;

        public QpScheduler(IUnityContainer unityContainer, List<QaConfigCustomer> customers)
        {
            if (unityContainer == null)
            {
                throw new ArgumentNullException(nameof(unityContainer));
            }

            _unityContainer = unityContainer;
            _customers = customers;
            _prtgLogger = LogProvider.GetLogger(LoggerData.DefaultPrtgLoggerName);
        }

        public void ParallelRun()
        {
            var exceptions = new ConcurrentQueue<Exception>();
            Parallel.ForEach(_customers, customer =>
            {
                try
                {
                    new DbScheduler(customer.ConnectionString, _unityContainer).Run();
                }
                catch (Exception ex)
                {
                    Logger.Log.Error($"There was an error on customer code: {customer.CustomerName}", ex);
                    exceptions.Enqueue(ex);
                }
            });

            if (exceptions.Any())
            {
                _prtgLogger.Error("There was an error at article scheduler service.", exceptions);
            }

            _prtgLogger.Info("All tasks successfully proceed.");
        }
    }
}
