using Microsoft.Practices.Unity;
using Quantumart.QP8.Logging.Services;
using Quantumart.QP8.Logging.Web.Services;

namespace Quantumart.QP8.Logging.Web
{
    public class LogServicesContainerConfigutation : UnityContainerExtension
    {
        protected override void Initialize()
        {
            Container.RegisterType<ILogReader, LogReader>();
        }
    }
}
