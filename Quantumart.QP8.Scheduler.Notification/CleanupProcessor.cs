using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Scheduler.API;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

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

		#region IProcessor implementation
		public async Task Run(CancellationToken token)
		{
			_logger.TraceInformation("Start cleanup notification queue");
			foreach (var connection in _connectionStrings)
			{
				using (var scope = new QPConnectionScope(connection))
				{
					_logger.TraceInformation("Cleanup notification queue for: " + connection);
					_externalNotificationService.DeleteSentNotifications();
					await Task.Delay(DelayDuration, token);
				}
			}
			_logger.TraceInformation("End cleanup notification queue");
		}
		#endregion
	}
}
