using System;
using System.IO;
using AutoMapper;
using Microsoft.Practices.Unity;
using QP8.Infrastructure.Logging.PrtgMonitoring.NLogExtensions.Factories;
using QP8.Infrastructure.Logging.PrtgMonitoring.NLogExtensions.Interfaces;
using Quantumart.QP8.BLL.Logging;
using Quantumart.QP8.BLL.Services.NotificationSender;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Scheduler.API;
using Quantumart.QP8.Scheduler.Notification.Processors;
using Quantumart.QP8.Scheduler.Notification.Providers;

namespace Quantumart.QP8.Scheduler.Notification
{
    public class SchedulerNotificationConfiguration : UnityContainerExtension
    {
        public const string ServiceName = "qp8.notification";

        protected override void Initialize()
        {
            Container.RegisterService(ServiceName, "QP8 Notification", "Отправка уведомлений");
            Container.RegisterType<IInterfaceNotificationProvider, InterfaceNotificationProvider>();

            RegisterSystemNotificationsProcessor("System.Notifications");
            RegisterInterfaceNotificationsProcessor("Interface.Notifications");

            Mapper.AddProfile<NotificationMapperProfile>();
        }

        private void RegisterSystemNotificationsProcessor(string serviceConfigName)
        {
            var nlogConfigName = $"NLog.{serviceConfigName}.config";
            Container.RegisterProcessor<SystemNotificationProcessor>(ServiceName, serviceConfigName);
            Container.RegisterType<IPrtgNLogFactory>(nlogConfigName, new InjectionFactory(container => GetLoggerFactory(GetAbsolutePath(nlogConfigName))));
            Container.RegisterType<IExternalSystemNotificationService, ExternalSystemNotificationService>(serviceConfigName);

            var assemblyType = typeof(SystemNotificationProcessor);
            Container.RegisterType<SystemNotificationProcessor>(
                assemblyType.Name,
                new TransientLifetimeManager(),
                new InjectionFactory(c => new SystemNotificationProcessor(
                        Container.Resolve<IPrtgNLogFactory>(nlogConfigName).GetLogger(assemblyType),
                        new PrtgErrorsHandler(Container.Resolve<IPrtgNLogFactory>(nlogConfigName)),
                        c.Resolve<ISchedulerCustomers>(),
                        c.Resolve<IExternalSystemNotificationService>(serviceConfigName)
                    )
                ));

            RegisterSystemCleanupProcessor($"{serviceConfigName}.Cleanup");
        }

        private void RegisterSystemCleanupProcessor(string serviceConfigName)
        {
            var nlogConfigName = $"NLog.{serviceConfigName}.config";
            Container.RegisterProcessor<SystemCleanupProcessor>(ServiceName, serviceConfigName);
            Container.RegisterType<IPrtgNLogFactory>(nlogConfigName, new InjectionFactory(container => GetLoggerFactory(GetAbsolutePath(nlogConfigName))));
            Container.RegisterType<IExternalSystemNotificationService, ExternalSystemNotificationService>(serviceConfigName);

            var assemblyType = typeof(SystemCleanupProcessor);
            Container.RegisterType<SystemCleanupProcessor>(
                assemblyType.Name,
                new TransientLifetimeManager(),
                new InjectionFactory(c => new SystemCleanupProcessor(
                        Container.Resolve<IPrtgNLogFactory>(nlogConfigName).GetLogger(assemblyType),
                        new PrtgErrorsHandler(Container.Resolve<IPrtgNLogFactory>(nlogConfigName)),
                        c.Resolve<ISchedulerCustomers>(),
                        c.Resolve<IExternalSystemNotificationService>(serviceConfigName)
                    )
                ));
        }

        private void RegisterInterfaceNotificationsProcessor(string serviceConfigName)
        {
            var nlogConfigName = $"NLog.{serviceConfigName}.config";
            Container.RegisterProcessor<InterfaceNotificationProcessor>(ServiceName, serviceConfigName);
            Container.RegisterType<IPrtgNLogFactory>(nlogConfigName, new InjectionFactory(container => GetLoggerFactory(GetAbsolutePath(nlogConfigName))));
            Container.RegisterType<IExternalInterfaceNotificationService, ExternalInterfaceNotificationService>(serviceConfigName);

            var assemblyType = typeof(InterfaceNotificationProcessor);
            Container.RegisterType<InterfaceNotificationProcessor>(
                assemblyType.Name,
                new TransientLifetimeManager(),
                new InjectionFactory(c => new InterfaceNotificationProcessor(
                        Container.Resolve<IPrtgNLogFactory>(nlogConfigName).GetLogger(assemblyType),
                        new PrtgErrorsHandler(Container.Resolve<IPrtgNLogFactory>(nlogConfigName)),
                        c.Resolve<ISchedulerCustomers>(),
                        c.Resolve<IExternalInterfaceNotificationService>(serviceConfigName),
                        c.Resolve<IInterfaceNotificationProvider>()
                    )
                ));

            RegisterInterfaceCleanupProcessor($"{serviceConfigName}.Cleanup");
        }

        private void RegisterInterfaceCleanupProcessor(string serviceConfigName)
        {
            var nlogConfigName = $"NLog.{serviceConfigName}.config";
            Container.RegisterProcessor<InterfaceCleanupProcessor>(ServiceName, serviceConfigName);
            Container.RegisterType<IPrtgNLogFactory>(nlogConfigName, new InjectionFactory(container => GetLoggerFactory(GetAbsolutePath(nlogConfigName))));
            Container.RegisterType<IExternalInterfaceNotificationService, ExternalInterfaceNotificationService>(serviceConfigName);

            var assemblyType = typeof(InterfaceCleanupProcessor);
            Container.RegisterType<InterfaceCleanupProcessor>(
                assemblyType.Name,
                new TransientLifetimeManager(),
                new InjectionFactory(c => new InterfaceCleanupProcessor(
                        Container.Resolve<IPrtgNLogFactory>(nlogConfigName).GetLogger(assemblyType),
                        new PrtgErrorsHandler(Container.Resolve<IPrtgNLogFactory>(nlogConfigName)),
                        c.Resolve<ISchedulerCustomers>(),
                        c.Resolve<IExternalInterfaceNotificationService>(serviceConfigName)
                    )
                ));
        }

        private static IPrtgNLogFactory GetLoggerFactory(string configPath) => new PrtgNLogFactory(
            configPath,
            LoggerData.DefaultPrtgServiceStateVariableName,
            LoggerData.DefaultPrtgServiceQueueVariableName,
            LoggerData.DefaultPrtgServiceStatusVariableName
        );

        private static string GetAbsolutePath(string relativePath) => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath);
    }
}
