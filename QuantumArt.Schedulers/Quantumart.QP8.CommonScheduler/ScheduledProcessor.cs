using System;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using Quantumart.QP8.Scheduler.API;

namespace Quantumart.QP8.CommonScheduler
{
    public class ScheduledProcessor : IProcessor
    {
        private readonly IProcessor _processor;
        private readonly ISchedule _schedule;
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private DateTime _lastCheckTime = DateTime.MinValue;
        private DateTime _lastStartTime = DateTime.MinValue;
        private DateTime _lastEndTime = DateTime.MinValue;

        public ScheduledProcessor(IProcessor processor, ISchedule schedule)
        {
            _processor = processor;
            _schedule = schedule;
        }

        public async Task Run(CancellationToken token)
        {
            var context = new SchedulerContext(DateTime.Now, _lastCheckTime, _lastStartTime, _lastEndTime);
            _lastCheckTime = DateTime.Now;

            if (_schedule.NeedProcess(context))
            {
                token.ThrowIfCancellationRequested();
                _lastStartTime = DateTime.Now;

                try
                {
                    await _processor.Run(token);
                }

                catch (Exception ex)
                {
                    Logger.Error(ex, "Scheduled task {task} run error", _processor.GetType());
                }

                _lastEndTime = DateTime.Now;
            }
        }
    }
}
