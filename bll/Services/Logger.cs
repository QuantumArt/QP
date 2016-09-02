using Quantumart.QP8.BLL.Factories.Logging;
using Quantumart.QP8.BLL.Interfaces.Logging;

namespace Quantumart.QP8.BLL.Services
{
    public class Logger
    {
        public static ILog Log { get; } = LogProvider.GetLogger();
    }
}
