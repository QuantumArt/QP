using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using QP8.Infrastructure.Logging;
using Quantumart.QP8.Scheduler.API;

namespace Quantumart.QP8.Scheduler.Core
{
    public sealed class Scheduler : IScheduler, IDisposable
    {
        private const string ServiceRepeatIntervalKey = "ServiceRepeatInterval";
        private const string ServiceRepeatOnErrorIntervalKey = "ServiceRepeatOnErrorInterval";

        private readonly IEnumerable<IProcessor> _processors;
        private readonly CancellationTokenSource _cts;
        private readonly TimeSpan _repeatInterval;
        private readonly TimeSpan _repeatOnErrorInterval;

        private Task[] Tasks { get; set; }

        public Scheduler(IEnumerable<IProcessor> processors)
        {
            if (!TimeSpan.TryParse(ConfigurationManager.AppSettings[ServiceRepeatIntervalKey], out _repeatInterval))
            {
                _repeatInterval = TimeSpan.FromSeconds(10);
            }

            if (!TimeSpan.TryParse(ConfigurationManager.AppSettings[ServiceRepeatOnErrorIntervalKey], out _repeatOnErrorInterval))
            {
                _repeatOnErrorInterval = TimeSpan.FromMinutes(10);
            }

            _processors = processors;
            _cts = new CancellationTokenSource();
        }

        public void Start()
        {
            Tasks = RunProcessors(_cts.Token).ToArray();
            Logger.Log.Info($"Service is started RepeatInterval = {_repeatInterval}, RepeatOnErrorInterval = {_repeatOnErrorInterval}");
        }

        private IEnumerable<Task> RunProcessors(CancellationToken cancellationToken)
        {
            return _processors.Select(processor => Task.Run(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    TimeSpan interval;
                    try
                    {
                        await processor.Run(cancellationToken);
                        interval = _repeatInterval;
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        Logger.Log.Fatal("Scheduler task run error", ex);
                        interval = _repeatOnErrorInterval;
                    }

                    await Task.Delay(interval, cancellationToken);
                }
            }, cancellationToken));
        }

        public void Stop()
        {
            Logger.Log.Info("Service is stoping...");
            _cts.Cancel();

            try
            {
                Task.WaitAll(Tasks);
            }
            catch (AggregateException)
            {
            }

            Logger.Log.Info("Service is stoped");
        }

        public void Dispose()
        {
            Logger.Log.Dispose();
            _cts.Dispose();
        }
    }
}
