using System;
using Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Models;
using Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Providers.ConfigurationProvider;

namespace Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Factories
{
    internal class ConfigurationProviderFactory
    {
        internal IConfigurationProvider Create(ConfigurationSettings settings, string connectionString)
        {
            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                Console.WriteLine("Already have connection string, proceeding to playback.");
                return new DummyConfigurationProvider();
            }

            if (!string.IsNullOrWhiteSpace(settings.QpConfigurationPath))
            {
                Console.WriteLine("Parsing configuration file to find connection string.");
                return new FileConfigurationProvider(settings.QpConfigurationPath);
            }

            if (!string.IsNullOrWhiteSpace(settings.QpConfigurationServiceUrl) && !string.IsNullOrWhiteSpace(settings.QpConfigurationServiceToken))
            {
                Console.WriteLine("Retrieving connection string from QP configuration service.");
                return new ServiceConfigurationProvider(settings.QpConfigurationServiceUrl, settings.QpConfigurationServiceToken);
            }

            Console.WriteLine("Fallback to windows registry config retrieving. Works only on windows!");
            return new DummyConfigurationProvider();
        }
    }
}
