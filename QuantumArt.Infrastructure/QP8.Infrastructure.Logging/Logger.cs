using QP8.Infrastructure.Logging.Factories;
using QP8.Infrastructure.Logging.Interfaces;

namespace QP8.Infrastructure.Logging
{
    public class Logger
    {
        public static ILog Log { get; set; } = LogProvider.GetLogger();
    }
}
