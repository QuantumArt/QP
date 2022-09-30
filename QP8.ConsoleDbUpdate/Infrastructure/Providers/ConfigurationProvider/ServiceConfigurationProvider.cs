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
        private readonly BaseSettingsModel _settings;

        public ServiceConfigurationProvider(BaseSettingsModel settings)
        {
            _settings = settings;
        }

        public void UpdateSettings()
        {
            CachedQPConfigurationService service = new(_settings.QpConfigUrl, _settings.QpConfigToken, TimeSpan.FromDays(1));

            CustomerConfiguration customer = AsyncHelper.RunSync(() => service.GetCustomer(_settings.CustomerCode));

            if (customer == null)
            {
                throw new OptionException($"No customer code in configuration service", _settings.CustomerCode);
            }

            _settings.ConnectionString = customer.ConnectionString;
            _settings.DbType = (DatabaseType)(int)customer.DbType;
        }
    }
}
