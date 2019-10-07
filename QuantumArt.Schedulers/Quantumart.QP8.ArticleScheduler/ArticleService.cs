using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Quantumart.QP8.ArticleScheduler
{
    #region snippet1
    public class ArticleService : IHostedService
    {
        private readonly ILogger _logger;
        private readonly IApplicationLifetime _appLifetime;
        internal readonly QpSchedulerProcessor Processor;
        private readonly ArticleSchedulerProperties _options;

        public ArticleService(
            ILogger<ArticleService> logger, IApplicationLifetime appLifetime, IOptions<ArticleSchedulerProperties> optionsAccessor)
        {
            _logger = logger;
            _appLifetime = appLifetime;
            _options = optionsAccessor.Value;
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
            _logger.LogInformation("Starting...");
            Processor.Run();
        }

        private void OnStopped()
        {
            _logger.LogInformation("Stopping...");
            Processor.Stop();
        }
    }
    #endregion
}
