using Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Models;

namespace Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Providers.ConfigurationProvider
{
    internal interface IConfigurationProvider
    {
        ApplicationSettings UpdateSettings(ApplicationSettings settings);
    }
}
