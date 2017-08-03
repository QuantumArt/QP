using Quantumart.QP8.Scheduler.API;

namespace Quantumart.QP8.Scheduler.Core
{
    public class NullSchedule : ISchedule
    {
        public bool NeedProcess(SchedulerContext context) => true;
    }
}
