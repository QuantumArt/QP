using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using QP8.Infrastructure.Logging.PrtgMonitoring.Data;
using QP8.Infrastructure.Logging.PrtgMonitoring.Interfaces;

namespace QP8.Infrastructure.Logging.PrtgMonitoring
{
    public class PrtgMonitoringService : IPrtgMonitoringService
    {
        private readonly string _host;
        private readonly string _identificationToken;

        public PrtgMonitoringService(string host, string identificationToken)
        {
            _host = host;
            _identificationToken = identificationToken;
        }

        public Url BaseUrl => _host.AppendPathSegment(_identificationToken);

        public async Task<PrtgHttpResponse> SendJsonRequest(object serializableLogDto) =>
            await BaseUrl.PostJsonAsync(serializableLogDto).ReceiveJson<PrtgHttpResponse>();

        public async Task<PrtgHttpResponse> SendStringGetRequest(string logMessage) =>
            await BaseUrl.SetQueryParams(new { Content = logMessage }).GetJsonAsync<PrtgHttpResponse>();

        public async Task<PrtgHttpResponse> SendStringPostRequest(string logMessage) =>
            await BaseUrl.PostStringAsync(logMessage).ReceiveJson<PrtgHttpResponse>();
    }
}
