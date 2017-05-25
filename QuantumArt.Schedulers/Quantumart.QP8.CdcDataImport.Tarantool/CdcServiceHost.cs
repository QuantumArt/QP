using AutoMapper;
using QP8.Infrastructure.Logging;
using Quantumart.QP8.CdcDataImport.Tarantool.Infrastructure;
using Quartz;
using Topshelf;

namespace Quantumart.QP8.CdcDataImport.Tarantool
{
    internal class CdcServiceHost : ServiceControl
    {
        private readonly IScheduler _scheduler;

        public CdcServiceHost(IScheduler scheduler)
        {
            _scheduler = scheduler;
        }

        public bool Start(HostControl hostControl)
        {
            if (!_scheduler.IsStarted)
            {
                _scheduler.Start();
            }

            Mapper.Initialize(cfg => { cfg.AddProfile<TarantoolMapperProfile>(); });
            Logger.Log.Debug("Service was started");
            return true;
        }

        public bool Stop(HostControl hostControl)
        {
            _scheduler.Shutdown(true);
            Logger.Log.Debug("Service was stopped");
            return true;
        }
    }
}
