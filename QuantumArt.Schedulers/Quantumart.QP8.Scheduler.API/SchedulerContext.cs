using System;

namespace Quantumart.QP8.Scheduler.API
{
    public sealed class SchedulerContext
    {
        public DateTime CurrentTime { get; }

        public DateTime LastCheckTime { get; }

        public DateTime LastStartTime { get; }

        public DateTime LastEndTime { get; }

        public SchedulerContext(DateTime currentTime, DateTime lastCheckTime, DateTime lastStartTime, DateTime lastEndTime)
        {
            CurrentTime = currentTime;
            LastCheckTime = lastCheckTime;
            LastStartTime = lastStartTime;
            LastEndTime = lastEndTime;
        }
    }
}
