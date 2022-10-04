using System;
using Mono.Options;
using QP.ConfigurationService.Client;
using QP.ConfigurationService.Models;
using Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Models;
using Quantumart.QP8.Utils;
using DatabaseType = Quantumart.QP8.Constants.DatabaseType;

namespace Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Providers.ConfigurationProvider
{
    internal class ServiceConfigurationProvider : IConfigurationProvider
    {
        private readonly string _qpConfigurationServiceUrl;
        private readonly string _qpConfigurationServiceToken;

        public ServiceConfigurationProvider(string qpConfigurationServiceUrl, string qpConfigurationServiceToken)
        {
            _qpConfigurationServiceUrl = qpConfigurationServiceUrl;
            _qpConfigurationServiceToken = qpConfigurationServiceToken;
        }

        public ApplicationSettings UpdateSettings(ApplicationSettings settings)
        {
            CachedQPConfigurationService service = new(_qpConfigurationServiceUrl, _qpConfigurationServiceToken, TimeSpan.FromDays(1));

            CustomerConfiguration customer = AsyncHelper.RunSync(() => service.GetCustomer(settings.CustomerCode));

            if (customer == null)
            {
                throw new OptionException($"No customer code in configuration service", settings.CustomerCode);
            }

            settings.ConnectionString = customer.ConnectionString;
            settings.DbType = (DatabaseType)(int)customer.DbType;

            return settings;
        }
    }
}
