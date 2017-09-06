using System.Threading.Tasks;
using Quantumart.QP8.Scheduler.Notification.Data;
using System.Net;

namespace Quantumart.QP8.Scheduler.Notification.Providers
{
    public interface IInterfaceNotificationProvider
    {
        Task<HttpStatusCode> Notify(InterfaceNotificationModel notification);
    }
}
