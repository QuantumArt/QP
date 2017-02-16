using System;
using System.Threading;
using System.Threading.Tasks;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Interfaces.Logging;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.BLL.Services.UserSynchronization;
using Quantumart.QP8.Scheduler.API;

namespace Quantumart.QP8.Scheduler.Users
{
    public class UsersProcessor : IProcessor, IDisposable
    {
        private const int DelayDuration = 100;

        private readonly ILog _logger;
        private readonly IConnectionStrings _connectionStrings;
        private readonly Func<IUserSynchronizationService> _getSynchronizationService;

        public UsersProcessor(ILog logger, IConnectionStrings connectionStrings, Func<IUserSynchronizationService> getSynchronizationService)
        {
            _logger = logger;
            _connectionStrings = connectionStrings;
            _getSynchronizationService = getSynchronizationService;
        }

        public async Task Run(CancellationToken token)
        {
            _logger.Info("Start users synchronization");
            foreach (var connection in _connectionStrings)
            {
                using (new QPConnectionScope(connection))
                {
                    var service = _getSynchronizationService();
                    if (service.NeedSynchronization())
                    {
                        _logger.Info($"Synchronization for: ${connection}");
                        service.Synchronize();
                    }
                }

                await Task.Delay(DelayDuration, token);
            }

            _logger.Info("End users synchronization");
        }

        public void Dispose()
        {
            Logger.Log.Dispose();
        }
    }
}
