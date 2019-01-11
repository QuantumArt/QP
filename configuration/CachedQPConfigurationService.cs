using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Quantumart.QP8.Configuration.Models;
using LazyCache;
using Refit;

namespace Quantumart.QP8.Configuration
{
    public class CachedQPConfigurationService : IQPConfigurationService
    {
        private readonly IQPConfigurationService _service;
        private readonly CachingService _cache;
        private readonly TimeSpan _period;

        public CachedQPConfigurationService(string serviceUrl, string jwtToken)
            : this(serviceUrl, jwtToken, TimeSpan.FromMinutes(5))
        {
        }

        public CachedQPConfigurationService(string serviceUrl, string jwtToken, TimeSpan cachePeriod)
        {
            _service = RestService.For<IQPConfigurationService>(
                new HttpClient(new JwtHttpClientHandler(jwtToken))
                {
                    BaseAddress = new Uri(serviceUrl)
                });

            // uses shared MemoryCache.Default under the hood
            _cache = new CachingService();

            _period = cachePeriod;
        }

        // TODO:
        //catch (ValidationApiException validationException)
        //{
        //   // handle validation here by using validationException.Content, 
        //   // which is type of ProblemDetails according to RFC 7807
        //}
        //catch (ApiException exception)
        //{
        //   // other exception handling
        //}
        
        public Task<QaConfigCustomer> GetCustomer(string customerCode)
        {
            return _cache.GetOrAddAsync(
                $"QPConfigurationService/customers/{customerCode}",
                () => _service.GetCustomer(customerCode),
                DateTimeOffset.Now.Add(_period));
        }

        public Task<List<QaConfigCustomer>> GetCustomers()
        {
            return _cache.GetOrAddAsync(
                $"QPConfigurationService/customers",
                () => _service.GetCustomers(),
                DateTimeOffset.Now.Add(_period));
        }

        public Task<List<QaConfigApplicationVariable>> GetVariables()
        {
            return _cache.GetOrAddAsync(
                $"QPConfigurationService/variables",
                () => _service.GetVariables(),
                DateTimeOffset.Now.Add(_period));
        }
        
        private class JwtHttpClientHandler : HttpClientHandler
        {
            private readonly string _jwtToken;

            public JwtHttpClientHandler(string jwtToken)
            {
                _jwtToken = jwtToken;
            }

            protected override async Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request, CancellationToken cancellationToken)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _jwtToken);

                return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
