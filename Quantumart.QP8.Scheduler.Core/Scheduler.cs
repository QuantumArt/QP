using Quantumart.QP8.Scheduler.API;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Quantumart.QP8.Scheduler.Core
{
	public sealed class Scheduler : IScheduler, IDisposable
	{
		private const string ServiceRepeatIntervalKey = "ServiceRepeatInterval";
		private const string ServiceRepeatOnErrorIntervalKey = "ServiceRepeatOnErrorInterval";

		private readonly IEnumerable<IProcessor> _processors;
		private readonly TraceSource _logger;
		private readonly CancellationTokenSource _cts;
		private readonly TimeSpan _repeatInterval;
		private readonly TimeSpan _repeatOnErrorInterval;
		private Task[] tasks { get; set; }

		public Scheduler(IEnumerable<IProcessor> processors, TraceSource logger)
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
			_logger = logger;
			_cts = new CancellationTokenSource();
		}

		public void Start()
		{			
			tasks = RunProcessors(_cts.Token).ToArray();
			_logger.TraceInformation("Service is started RepeatInterval = {0}, RepeatOnErrorInterval = {1}", _repeatInterval, _repeatOnErrorInterval);
			_logger.Flush();	
		}

		private IEnumerable<Task> RunProcessors(CancellationToken cancellationToken)
		{
			foreach (var p in _processors)
			{
				var processor = p;

				yield return Task.Run(async () =>
				{
					TimeSpan interval;

					while (!cancellationToken.IsCancellationRequested)
					{
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
							_logger.TraceData(TraceEventType.Error, EventIdentificators.Common, ex);
							interval = _repeatOnErrorInterval;
						}

						_logger.Flush();
						await Task.Delay(interval, cancellationToken);
					}
				}, cancellationToken);
			}
		}

		public void Stop()
		{
			_logger.TraceInformation("Service is stoping...");
			_cts.Cancel();
			
			try
			{
				Task.WaitAll(tasks);
			}
			catch (AggregateException)
			{
			}

			_logger.TraceInformation("Service is stoped");
			_logger.Flush();			
		}

		public void Dispose()
		{
			_logger.Close();
			_cts.Dispose();
		}
	}
}