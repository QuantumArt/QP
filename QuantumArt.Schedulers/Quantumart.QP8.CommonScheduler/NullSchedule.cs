using Quantumart.QP8.Scheduler.API;

namespace Quantumart.QP8.CommonScheduler
{
    public class NullSchedule : ISchedule
    {
        public bool NeedProcess(SchedulerContext context) => true;
    }
}
