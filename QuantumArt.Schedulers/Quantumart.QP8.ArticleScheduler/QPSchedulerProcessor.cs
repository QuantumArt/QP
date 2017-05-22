using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using QP8.Infrastructure.Logging;
using Quantumart.QP8.Configuration;

namespace Quantumart.QP8.ArticleScheduler
{
    public class QpSchedulerProcessor
    {
        private const string AppName = "QP8ArticleSchedulerService";

        private readonly TimeSpan _recurrentTimeout;
        private readonly TimeSpan _tasksQueueCheckShiftTime;

        private Task _task;
        private CancellationTokenSource _cancellationTokenSource;

        public QpSchedulerProcessor(TimeSpan recurrentTimeout, TimeSpan tasksQueueCheckShiftTime)
        {
            _recurrentTimeout = recurrentTimeout;
            _tasksQueueCheckShiftTime = tasksQueueCheckShiftTime;
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
                        var unityConfig = new UnityContainerCustomizer();
                        var customers = QPConfiguration.GetCustomers(AppName).Where(c => !c.ExcludeFromSchedulers).ToList();
                        new QpScheduler(unityConfig.UnityContainer, customers, _tasksQueueCheckShiftTime).Run();
                    }
                    catch (Exception ex)
                    {
                        Logger.Log.Error("There was an error while starting the service job", ex);
                    }
                } while (!_cancellationTokenSource.Token.WaitHandle.WaitOne(_recurrentTimeout));
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
                Logger.Log.Error("There was an error while stopping the service", ex);
            }
            finally
            {
                _cancellationTokenSource.Dispose();
                _task.Dispose();
            }
        }
    }
}
