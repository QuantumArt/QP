using System;
using System.Threading;
using System.Threading.Tasks;
using Quantumart.QP8.Scheduler.API;

namespace Quantumart.QP8.Scheduler.Core
{
    public class ScheduledProcessor : IProcessor
    {
        private readonly Func<IProcessor> _getProcessor;
        private readonly Func<ISchedule> _getSchedule;
        private DateTime _lastCheckTime = DateTime.MinValue;
        private DateTime _lastStartTime = DateTime.MinValue;
        private DateTime _lastEndTime = DateTime.MinValue;

        public ScheduledProcessor(Func<IProcessor> getProcessor, Func<ISchedule> getSchedule)
        {
            _getProcessor = getProcessor;
            _getSchedule = getSchedule;
        }

        public async Task Run(CancellationToken token)
        {
            var context = new SchedulerContext(DateTime.Now, _lastCheckTime, _lastStartTime, _lastEndTime);
            _lastCheckTime = DateTime.Now;

            if (_getSchedule().NeedProcess(context))
            {
                token.ThrowIfCancellationRequested();
                _lastStartTime = DateTime.Now;
                await _getProcessor().Run(token);
                _lastEndTime = DateTime.Now;
            }
        }
    }
}
