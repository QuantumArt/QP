using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using Quantumart.QP8.Configuration;

namespace Quantumart.QP8.ArticleScheduler
{
    /// <summary>
    /// Запускает QPScheduler в отдельном потоке
    /// </summary>
    public class QpSchedulerProcessor
    {
        private CancellationTokenSource _cancellationTokenSource;
        private Task _task;

        private readonly TimeSpan _recurrentTimeout;
        private readonly IEnumerable<string> _exceptCustomerCodes;

        private const string AppName = "QP8ArticleSchedulerService";

        public QpSchedulerProcessor(TimeSpan recurrentTimeout, IEnumerable<string> exceptCustomerCodes)
        {
            _recurrentTimeout = recurrentTimeout;
            _exceptCustomerCodes = exceptCustomerCodes ?? new string[0];
        }

        public void Run()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _task = new Task(() =>
            {
                do
                {
                    try
                    {
                        new QpScheduler(QPConfiguration.GetConnectionStrings(AppName, _exceptCustomerCodes), UnityContainerCustomizer.UnityContainer).ParallelRun();
                    }
                    catch (Exception exp)
                    {
                        UnityContainerCustomizer.UnityContainer.Resolve<IExceptionHandler>().HandleException(exp);
                    }
                }
                while (!_cancellationTokenSource.Token.WaitHandle.WaitOne(_recurrentTimeout));
            }, _cancellationTokenSource.Token, TaskCreationOptions.LongRunning);
            _task.Start();
        }

        public void Stop()
        {
            try
            {
                _cancellationTokenSource.Cancel();
                _task.Wait();
            }
            catch (Exception exp)
            {
                UnityContainerCustomizer.UnityContainer.Resolve<IExceptionHandler>().HandleException(exp);
            }
            finally
            {
                _cancellationTokenSource.Dispose();
                _task.Dispose();
                _cancellationTokenSource = null;
                _task = null;
            }
        }
    }
}
