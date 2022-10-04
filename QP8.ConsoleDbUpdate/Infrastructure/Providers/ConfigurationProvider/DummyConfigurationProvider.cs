using Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Models;

namespace Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Providers.ConfigurationProvider
{
    internal class DummyConfigurationProvider : IConfigurationProvider
    {
        public ApplicationSettings UpdateSettings(ApplicationSettings settings)
        {
            return settings;
        }
    }
}
