using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using NLog;

namespace Quantumart.QP8.ArticleScheduler
{
    #region snippet1
    public class ArticleService : IHostedService
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private readonly IApplicationLifetime _appLifetime;
        internal readonly QpSchedulerProcessor Processor;
        private readonly ArticleSchedulerProperties _options;

        public ArticleService(IApplicationLifetime appLifetime, ArticleSchedulerProperties options)
        {
            _appLifetime = appLifetime;
            _options = options;
            Processor = new QpSchedulerProcessor(_options);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _appLifetime.ApplicationStarted.Register(OnStarted);
            _appLifetime.ApplicationStopped.Register(OnStopped);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private void OnStarted()
        {
            Logger.Info("Starting article scheduler...");
            Processor.Run();
        }

        private void OnStopped()
        {
            Logger.Info("Stopping article scheduler...");
            Processor.Stop();
        }
    }
    #endregion
}
