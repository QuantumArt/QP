using System;
using System.Threading;
using System.Threading.Tasks;
using QP8.Infrastructure.Logging;
using QP8.Infrastructure.Logging.PrtgMonitoring.Data;
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
                        var customers = QPConfiguration.GetCustomers(AppName, true);
                        new QpScheduler(unityConfig.UnityContainer, customers, _tasksQueueCheckShiftTime).Run();
                    }
                    catch (Exception ex)
                    {
                        const string errorMessage = "There was an error while starting the service job";
                        Logger.Log.Error(errorMessage, ex);
                        PrtgErrorsHandler.LogMessage(new PrtgServiceMonitoringMessage
                        {
                            Message = errorMessage,
                            State = PrtgServiceMonitoringEnum.CriticalError
                        });
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
                const string errorMessage = "There was an error while stopping the service";
                Logger.Log.Error(errorMessage, ex);
                PrtgErrorsHandler.LogMessage(new PrtgServiceMonitoringMessage
                {
                    Message = errorMessage,
                    State = PrtgServiceMonitoringEnum.CriticalError
                });
            }
            finally
            {
                _cancellationTokenSource.Dispose();
                _task.Dispose();
            }
        }
    }
}
