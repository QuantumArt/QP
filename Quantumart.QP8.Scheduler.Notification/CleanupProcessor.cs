using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Scheduler.API;

namespace Quantumart.QP8.Scheduler.Notification
{
    public class CleanupProcessor : IProcessor
    {
        private const int DelayDuration = 100;
        private readonly TraceSource _logger;
        private readonly IConnectionStrings _connectionStrings;
        private readonly IExternalNotificationService _externalNotificationService;

        public CleanupProcessor(
            TraceSource logger,
            IConnectionStrings connectionStrings,
            IExternalNotificationService externalNotificationService)
        {
            _logger = logger;
            _connectionStrings = connectionStrings;
            _externalNotificationService = externalNotificationService;
        }

        public async Task Run(CancellationToken token)
        {
            _logger.TraceInformation("Start cleanup notification queue");
            foreach (var connection in _connectionStrings)
            {
                var builder = new SqlConnectionStringBuilder(connection);
                using (new QPConnectionScope(connection))
                {
                    if (_externalNotificationService.ExistsSentNotifications())
                    {
                        _logger.TraceInformation($"Cleanup notification queue for database {builder.InitialCatalog} on server {builder.DataSource}");
                        _externalNotificationService.DeleteSentNotifications();
                    }
                }

                await Task.Delay(DelayDuration, token);
            }

            _logger.TraceInformation("End cleanup notification queue");
        }
    }
}
