using System.Threading.Tasks;
using QP8.Infrastructure.Logging.PrtgMonitoring.Data;

namespace QP8.Infrastructure.Logging.PrtgMonitoring.Interfaces
{
    public interface IPrtgMonitoringService
    {
        Task<PrtgHttpResponse> SendJsonRequest(object serializableLogDto);

        Task<PrtgHttpResponse> SendStringGetRequest(string logMessage);

        Task<PrtgHttpResponse> SendStringPostRequest(string logMessage);
    }
}
