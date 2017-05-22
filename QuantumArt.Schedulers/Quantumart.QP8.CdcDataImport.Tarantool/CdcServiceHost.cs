using AutoMapper;
using QP8.Infrastructure.Logging;
using Quantumart.QP8.CdcDataImport.Tarantool.Infrastructure;

namespace Quantumart.QP8.CdcDataImport.Tarantool
{
    internal class CdcServiceHost
    {
        public void Start()
        {
            Mapper.Initialize(cfg => { cfg.AddProfile<TarantoolMapperProfile>(); });
            Logger.Log.Debug("Service was started");
        }

        public void Stop()
        {
            Logger.Log.Debug("Service was stopped");
        }
    }
}
