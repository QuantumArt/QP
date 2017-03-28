using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using QP8.Infrastructure.Logging.Factories;

namespace Quantumart.QP8.ArticleScheduler
{
    public class QpScheduler
    {
        private readonly IEnumerable<string> _connectionStrings;
        private readonly IUnityContainer _unityContainer;

        public QpScheduler(IEnumerable<string> connectionStrings, IUnityContainer unityContainer)
        {
            if (unityContainer == null)
            {
                throw new ArgumentNullException(nameof(unityContainer));
            }

            _connectionStrings = connectionStrings;
            _unityContainer = unityContainer;
        }

        public void ParallelRun()
        {
            try
            {
                var exceptions = new ConcurrentQueue<Exception>();
                Parallel.ForEach(_connectionStrings, cs =>
                {
                    try
                    {
                        new DbScheduler(cs, _unityContainer).Run();
                    }
                    catch (Exception ex)
                    {
                        exceptions.Enqueue(ex);
                    }
                });

                if (exceptions.Any())
                {
                    throw new AggregateException(exceptions);
                }

                LogProvider.GetLogger("prtg").Info("PRTG Ok.");
            }
            catch (Exception ex)
            {
                UnityContainerCustomizer.UnityContainer.Resolve<IExceptionHandler>().HandleException(ex);
            }
        }
    }
}
