using Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Adapters;
using Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Models;
using Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Providers.ConfigurationProvider;

namespace Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Factories
{
    internal class ConfigurationProviderFactory
    {
        internal static IConfigurationProvider Create(BaseSettingsModel settings, QpUpdateLoggingWrapper logger)
        {
            if (!string.IsNullOrWhiteSpace(settings.ConnectionString))
            {
                return new DummyConfigurationProvider("Already have connection string, proceeding to playback.", logger);
            }

            if (!string.IsNullOrWhiteSpace(settings.QpConfigPath))
            {
                return new FileConfigurationProvider(settings);
            }

            if (!string.IsNullOrWhiteSpace(settings.QpConfigUrl) && !string.IsNullOrWhiteSpace(settings.QpConfigToken))
            {
                return new ServiceConfigurationProvider(settings);
            }

            return new DummyConfigurationProvider("Fallback to windows registry config retrieving. Works only on windows!", logger);
        }
    }
}
