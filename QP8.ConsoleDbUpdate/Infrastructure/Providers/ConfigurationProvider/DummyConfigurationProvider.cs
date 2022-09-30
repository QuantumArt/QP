using Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Adapters;

namespace Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Providers.ConfigurationProvider
{
    internal class DummyConfigurationProvider : IConfigurationProvider
    {
        private readonly string _message;
        private readonly QpUpdateLoggingWrapper _logger;

        public DummyConfigurationProvider(string message, QpUpdateLoggingWrapper logger)
        {
            _message = message;
            _logger = logger;
        }

        public void UpdateSettings()
        {
            _logger.Info(_message);
        }
    }
}
