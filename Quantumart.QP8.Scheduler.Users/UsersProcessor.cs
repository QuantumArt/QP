using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services.UserSynchronization;
using Quantumart.QP8.Scheduler.API;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Quantumart.QP8.Scheduler.Users
{
	public class UsersProcessor : IProcessor
	{
		private const int DelayDuration = 100;
		private readonly TraceSource _logger;
		private readonly IConnectionStrings _connectionStrings;
		private readonly Func<IUserSynchronizationService> _getSynchronizationService;

		public UsersProcessor(
			TraceSource logger,
			IConnectionStrings connectionStrings,
			Func<IUserSynchronizationService> getSynchronizationService)
		{
			_logger = logger;
			_connectionStrings = connectionStrings;
			_getSynchronizationService = getSynchronizationService;
		}

		public async Task Run(CancellationToken token)
		{
			_logger.TraceInformation("Start users synchronization");

			foreach (var connection in _connectionStrings)
			{
				using (var scope = new QPConnectionScope(connection))
				{
					var service = _getSynchronizationService();

					if (service.NeedSynchronization())
					{
						_logger.TraceInformation("Synchronization for: " + connection);
						service.Synchronize();
					}
				}

				await Task.Delay(DelayDuration, token);
			}

			_logger.TraceInformation("End users synchronization");
		}
	}
}
