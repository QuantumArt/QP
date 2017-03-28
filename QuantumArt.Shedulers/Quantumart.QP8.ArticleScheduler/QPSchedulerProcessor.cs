using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using Quantumart.QP8.Configuration;

namespace Quantumart.QP8.ArticleScheduler
{
    public class QpSchedulerProcessor
    {
        private Task _task;
        private CancellationTokenSource _cancellationTokenSource;

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
                    catch (Exception ex)
                    {
                        UnityContainerCustomizer.UnityContainer.Resolve<IExceptionHandler>().HandleException(ex);
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
            catch (Exception ex)
            {
                UnityContainerCustomizer.UnityContainer.Resolve<IExceptionHandler>().HandleException(ex);
            }
            finally
            {
                _cancellationTokenSource.Dispose();
                _task.Dispose();
            }
        }
    }
}
