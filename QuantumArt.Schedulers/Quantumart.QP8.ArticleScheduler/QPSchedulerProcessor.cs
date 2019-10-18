using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using NLog.Fluent;
using QP8.Infrastructure.Logging.PrtgMonitoring.NLogExtensions.Interfaces;
using Quantumart.QP8.BLL.Logging;
using Quantumart.QP8.Configuration;
using Unity;

namespace Quantumart.QP8.ArticleScheduler
{
    public class QpSchedulerProcessor
    {
        private const string AppName = "QP8ArticleSchedulerService";
        private static ILogger Logger = LogManager.GetCurrentClassLogger();

        private readonly ArticleSchedulerProperties _props;
        private CancellationTokenSource _cancellationTokenSource;

        private Task _task;

        public QpSchedulerProcessor(ArticleSchedulerProperties props)
        {
            _props = props;
        }

        public void Run()
        {
            var unityConfig = new UnityContainerCustomizer();
            var prtgLogger = new PrtgErrorsHandler(unityConfig.UnityContainer.Resolve<IPrtgNLogFactory>());

            _cancellationTokenSource = new CancellationTokenSource();
            _task = new Task(() =>
            {
                do
                {
                    try
                    {
                        QPConfiguration.ConfigServiceUrl = _props.ConfigServiceUrl;
                        QPConfiguration.ConfigServiceToken = _props.ConfigServiceToken;
                        QPConfiguration.XmlConfigPath = _props.XmlConfigPath;

                        var customers = QPConfiguration.GetCustomers(AppName).Where(c => !c.ExcludeFromSchedulers).ToList();
                        new QpScheduler(unityConfig.UnityContainer, prtgLogger, customers, _props.PrtgLoggerTasksQueueCheckShiftTime).Run();
                    }
                    catch (Exception ex)
                    {
                        Logger.Error().Exception(ex).Message("There was an error while starting the service job").Write();
                    }
                } while (!_cancellationTokenSource.Token.WaitHandle.WaitOne(_props.RecurrentTimeout));
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
                Logger.Error().Exception(ex).Message("There was an error while stopping the service").Write();
            }
            finally
            {
                _cancellationTokenSource.Dispose();
                _task.Dispose();
            }
        }
    }
}
