using QP8.Infrastructure.Web.Enums;

namespace Quantumart.QP8.Scheduler.API.Models
{
    public class JobStatus
    {
        public JSendStatus Status { get; set; }
        public string Message { get; set; }
    }
}
