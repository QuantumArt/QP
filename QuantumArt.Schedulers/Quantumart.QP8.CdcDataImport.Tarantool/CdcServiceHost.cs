using QP8.Infrastructure;
using QP8.Infrastructure.Logging;
using Quartz;
using Topshelf;

namespace Quantumart.QP8.CdcDataImport.Tarantool
{
    internal class CdcServiceHost : ServiceControl
    {
        private readonly IScheduler _scheduler;

        public CdcServiceHost(IScheduler scheduler)
        {
            Ensure.Argument.NotNull(scheduler);
            _scheduler = scheduler;
        }

        public bool Start(HostControl hostControl)
        {
            if (!_scheduler.IsStarted)
            {
                _scheduler.Start();
            }

            Logger.Log.Debug("Service was started");
            return true;
        }

        public bool Stop(HostControl hostControl)
        {
            Logger.Log.Debug("Stopping scheduler");
            _scheduler.Shutdown(true);

            Logger.Log.Debug("Service was stopped");
            return true;
        }
    }
}
