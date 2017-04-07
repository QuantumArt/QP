using System;
using System.Threading;
using System.Threading.Tasks;
using QP8.Infrastructure.Logging;
using Quantumart.QP8.Configuration;

namespace Quantumart.QP8.ArticleScheduler
{
    public class QpSchedulerProcessor
    {
        private Task _task;
        private CancellationTokenSource _cancellationTokenSource;

        private readonly TimeSpan _recurrentTimeout;

        private const string AppName = "QP8ArticleSchedulerService";

        public QpSchedulerProcessor(TimeSpan recurrentTimeout)
        {
            _recurrentTimeout = recurrentTimeout;
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
                        var customers = QPConfiguration.GetCustomers(AppName, true);
                        new QpScheduler(UnityContainerCustomizer.UnityContainer, customers).Run();
                    }
                    catch (Exception ex)
                    {
                        Logger.Log.Error($"There was an error while starting the service job, {ex}");
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
                Logger.Log.Error($"There was an error while stopping the service, {ex}");
            }
            finally
            {
                _cancellationTokenSource.Dispose();
                _task.Dispose();
            }
        }
    }
}
