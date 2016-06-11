using System;

namespace Quantumart.QP8.Scheduler.API
{
	public sealed class SchedulerContext
	{
		public DateTime CurrentTime { get; private set; }
		public DateTime LastCheckTime { get; private set; }
		public DateTime LastStartTime { get; private set; }
		public DateTime LastEndTime { get; private set; }

		public SchedulerContext(DateTime currentTime, DateTime lastCheckTime, DateTime lastStartTime, DateTime lastEndTime)
		{
			CurrentTime = currentTime;
			LastCheckTime =lastCheckTime;
			LastStartTime =lastStartTime;
			LastEndTime = lastEndTime;
		}
	}
}