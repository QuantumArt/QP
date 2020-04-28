using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using NLog;
using NLog.Fluent;
using Quantumart.QP8.BLL.Services.CdcImport;
using Quantumart.QP8.BLL.Services.DbServices;
using Quantumart.QP8.BLL.Services.NotificationSender;
using Quantumart.QP8.BLL.Services.UserSynchronization;
using Quantumart.QP8.Scheduler.API;
using Quantumart.QP8.Scheduler.Notification.Processors;
using Quantumart.QP8.Scheduler.Notification.Providers;
using Quantumart.QP8.Scheduler.Users;
using T = Quantumart.QP8.CdcDataImport.Tarantool.Infrastructure.Jobs;
using E = Quantumart.QP8.CdcDataImport.Elastic.Infrastructure.Jobs;


namespace Quantumart.QP8.CommonScheduler
{
    public sealed class CommonService : IHostedService
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private readonly IHostApplicationLifetime _applicationLifetime;
        private readonly List<IProcessor> _processors;
        private CancellationToken _ct;
        private readonly TimeSpan _repeatInterval;
        private readonly TimeSpan _repeatOnErrorInterval;
        private readonly Dictionary<string, TimeSpan> _intervals;

        private Task[] Tasks { get; set; }

        public CommonService(
            IHostApplicationLifetime applicationLifetime,
            CommonSchedulerProperties props,
            ISchedulerCustomerCollection customerCollection,
            IExternalInterfaceNotificationService notifyService,
            IExternalSystemNotificationService systemNotifyService,
            IInterfaceNotificationProvider notifyProvider,
            ElasticCdcImportService esImportService,
            TarantoolCdcImportService tntImportService,
            IDbService dbService
        )
        {
            _applicationLifetime = applicationLifetime;
            _repeatInterval = props.ServiceRepeatInterval;
            _repeatOnErrorInterval = props.ServiceRepeatOnErrorInterval;
            _intervals = props.Tasks.ToDictionary(n => n.Name, m => m.Interval);
            _processors = new List<IProcessor>();

            AddIntervalScheduledProcessor("Notifications",
                () => new InterfaceNotificationProcessor(customerCollection, notifyService, notifyProvider)
            );

            AddIntervalScheduledProcessor("SystemNotifications",
                () => new SystemNotificationProcessor(customerCollection, systemNotifyService)
            );

            AddIntervalScheduledProcessor("Cleanup",
                () => new InterfaceCleanupProcessor(customerCollection, notifyService)
            );

            AddIntervalScheduledProcessor("SystemCleanup",
                () => new SystemCleanupProcessor(customerCollection, systemNotifyService)
            );

            AddIntervalScheduledProcessor("Users",
                () => new UsersProcessor(
                    customerCollection, new UserSynchronizationService(props.DefaultUserId, props.DefaultLanguageId)
                 )
            );

            AddIntervalScheduledProcessor("TarantoolCdc",
                () => new T.CdcDataImportProcessor(tntImportService, systemNotifyService)
            );

            AddIntervalScheduledProcessor("TarantoolNotifications",
                () => new T.CheckNotificationQueueProcessor(dbService, systemNotifyService)
            );

            AddIntervalScheduledProcessor("ElasticCdc",
                () => new E.CdcDataImportProcessor(esImportService, systemNotifyService)
            );

            AddIntervalScheduledProcessor("ElasticNotifications",
                () => new E.CheckNotificationQueueProcessor(dbService, systemNotifyService)
            );
        }

        private void AddIntervalScheduledProcessor(string key, Func<IProcessor> processorFunc)
        {
            if (_intervals.TryGetValue(key, out var interval))
            {
                AddIntervalScheduledProcessor(processorFunc(), interval);
            }
        }

        private void AddIntervalScheduledProcessor(IProcessor processor, TimeSpan interval)
        {
            _processors.Add(new ScheduledProcessor(processor, new IntervalSchedule(interval)));
        }

        private void OnStarted()
        {
            Logger.Info($"Common scheduler is starting...");
            Tasks = RunProcessors(_ct).ToArray();
            Logger.Info()
                .Message("Common scheduler started")
                .Property("RepeatInterval", _repeatInterval)
                .Property("RepeatOnErrorInterval", _repeatOnErrorInterval)
                .Write();
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
                    catch (Exception)
                    {
                        interval = _repeatOnErrorInterval;
                    }

                    await Task.Delay(interval, cancellationToken);
                }
            }, cancellationToken));
        }

        private void OnStopped()
        {
            Logger.Info("Common scheduler is stopping...");

            try
            {
                Task.WaitAll(Tasks);
            }
            catch (AggregateException)
            {
            }

            Logger.Info("Common scheduler stopped");
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _ct = cancellationToken;
            _applicationLifetime.ApplicationStarted.Register(OnStarted);
            _applicationLifetime.ApplicationStopped.Register(OnStopped);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

    }
}
