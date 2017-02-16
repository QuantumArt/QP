using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Scheduler.API;

namespace Quantumart.QP8.Scheduler.Notification
{
    public class CleanupProcessor : IProcessor, IDisposable
    {
        private const int DelayDuration = 100;
        private readonly IConnectionStrings _connectionStrings;
        private readonly IExternalNotificationService _externalNotificationService;

        public CleanupProcessor(IConnectionStrings connectionStrings, IExternalNotificationService externalNotificationService)
        {
            _connectionStrings = connectionStrings;
            _externalNotificationService = externalNotificationService;
        }

        public async Task Run(CancellationToken token)
        {
            Logger.Log.Info("Start cleanup notification queue");
            foreach (var connection in _connectionStrings)
            {
                var builder = new SqlConnectionStringBuilder(connection);
                using (new QPConnectionScope(connection))
                {
                    if (_externalNotificationService.ExistsSentNotifications())
                    {
                        Logger.Log.Info($"Cleanup notification queue for database {builder.InitialCatalog} on server {builder.DataSource}");
                        _externalNotificationService.DeleteSentNotifications();
                    }
                }

                await Task.Delay(DelayDuration, token);
            }

            Logger.Log.Info("End cleanup notification queue");
        }

        public void Dispose()
        {
            Logger.Log.Dispose();
        }
    }
}
