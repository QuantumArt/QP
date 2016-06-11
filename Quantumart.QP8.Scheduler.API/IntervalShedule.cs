using System;

namespace Quantumart.QP8.Scheduler.API
{
	public class IntervalSchedule : ISchedule
	{
		private readonly TimeSpan _interval;

		public IntervalSchedule(TimeSpan interval)
		{
			_interval = interval;
		}

		public bool NeedProcess(SchedulerContext context)
		{
			return context.CurrentTime - context.LastEndTime > _interval;
		}
	}
}