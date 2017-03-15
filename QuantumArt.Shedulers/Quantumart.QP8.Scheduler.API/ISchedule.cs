namespace Quantumart.QP8.Scheduler.API
{
    public interface ISchedule
    {
        bool NeedProcess(SchedulerContext context);
    }
}
