using AutoMapper;
using Flurl.Http;
using Flurl.Http.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using QP8.Infrastructure.Logging;
using Quantumart.QP8.CdcDataImport.Elastic.Infrastructure;

namespace Quantumart.QP8.CdcDataImport.Elastic
{
    internal class CdcServiceHost
    {
        public void Start()
        {
            Mapper.Initialize(cfg => { cfg.AddProfile<ElasticMapperProfile>(); });
            FlurlHttp.Configure(c =>
            {
                c.JsonSerializer = new NewtonsoftJsonSerializer(new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                });
            });

            Logger.Log.Debug("Service was started");
        }

        public void Stop()
        {
            Logger.Log.Debug("Service was stopped");
        }
    }
}
