using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using NLog.Fluent;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.ArticleScheduler
{
    public class QpSchedulerProcessor
    {
        private const string AppName = "QP8.ArticleScheduler";
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
            Logger.Info("QP8.ArticleScheduler starting...");
            var unityConfig = new UnityContainerCustomizer();

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

                        var customers = QPConfiguration.GetCustomers(AppName)
                            .Where(c => c.DbType == DatabaseType.SqlServer)
                            .Where(c => !c.ExcludeFromSchedulers)
                            .ToList();
                        new QpScheduler(unityConfig.UnityContainer, customers, _props.PrtgLoggerTasksQueueCheckShiftTime).Run();
                    }
                    catch (Exception ex)
                    {
                        Logger.Error().Exception(ex).Message("There was an error while starting the service job").Write();
                    }
                } while (!_cancellationTokenSource.Token.WaitHandle.WaitOne(_props.RecurrentTimeout));
            }, _cancellationTokenSource.Token, TaskCreationOptions.LongRunning);
            _task.Start();
            Logger.Info("QP8.ArticleScheduler started");
        }

        public void Stop()
        {
            var failed = false;
            Logger.Info("QP8.ArticleScheduler stopping...");
            try
            {
                _cancellationTokenSource.Cancel();
                _task.Wait();
            }
            catch (Exception ex)
            {
                Logger.Error().Exception(ex).Message("There was an error while stopping the service").Write();
                failed = true;
            }
            finally
            {
                _cancellationTokenSource.Dispose();
                _task.Dispose();
            }

            if (!failed)
            {
                Logger.Info("QP8.ArticleScheduler stopped");
            }

        }
    }
}
