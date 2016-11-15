using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.Unity;

namespace Quantumart.QP8.ArticleScheduler
{
    /// <summary>
    /// Класс выполняет задачи из всех БД
    /// </summary>
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

        /// <summary>
        /// Выполняет расписания
        /// </summary>
        public void ParallelRun()
        {
            _connectionStrings.AsParallel().ForAll(cs =>
            {
                try
                {
                    new DbScheduler(cs, _unityContainer).ParallelRun();
                }
                catch (Exception exp)
                {
                    UnityContainerCustomizer.UnityContainer.Resolve<IExceptionHandler>().HandleException(exp);
                }
            });
        }
    }
}
