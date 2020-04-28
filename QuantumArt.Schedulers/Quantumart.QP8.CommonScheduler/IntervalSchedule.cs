using System;
using Quantumart.QP8.Scheduler.API;

namespace Quantumart.QP8.CommonScheduler
{
    public class IntervalSchedule : ISchedule
    {
        private readonly TimeSpan _interval;

        public IntervalSchedule(TimeSpan interval)
        {
            _interval = interval;
        }

        public bool NeedProcess(SchedulerContext context) => context.CurrentTime - context.LastEndTime > _interval;
    }
}
