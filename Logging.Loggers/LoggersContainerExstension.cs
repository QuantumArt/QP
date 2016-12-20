using Microsoft.Practices.Unity;

namespace Quantumart.QP8.Logging.Loggers
{
    public static class LoggersContainerExstension
    {
        public static IUnityContainer RegisterLoggerModel<T>(this IUnityContainer container)
            where T : class
        {
            return container.RegisterType<ILogger<T>, Logger<T>>();
        }
    }
}
