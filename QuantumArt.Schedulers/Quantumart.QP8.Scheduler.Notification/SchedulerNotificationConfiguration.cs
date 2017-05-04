using Microsoft.Practices.Unity;
using QP8.Infrastructure.Logging.PrtgMonitoring.NLogExtensions.Factories;
using QP8.Infrastructure.Logging.PrtgMonitoring.NLogExtensions.Interfaces;
using Quantumart.QP8.BLL.Logging;
using Quantumart.QP8.BLL.Services.NotificationSender;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Scheduler.API;
using Quantumart.QP8.Scheduler.Notification.Processors;
using Quantumart.QP8.Scheduler.Notification.Providers;
using System;
using System.IO;

namespace Quantumart.QP8.Scheduler.Notification
{
    public class SchedulerNotificationConfiguration : UnityContainerExtension
    {
        public const string ServiceName = "qp8.notification";

        protected override void Initialize()
        {
            Container.RegisterService(ServiceName, "QP8 Notification", "Отправка уведомлений");
            Container.RegisterType<INotificationProvider, NotificationProvider>();

            RegisterSystemNotificationsProcessor("System.Notifications");
            RegisterInterfaceNotificationsProcessor("Interface.Notifications");
        }

        private void RegisterSystemNotificationsProcessor(string serviceName)
        {
            var nlogConfigName = $"NLog.{serviceName}.config";
            Container.RegisterProcessor<SystemNotificationProcessor>(ServiceName, serviceName);
            Container.RegisterType<IPrtgNLogFactory>(nlogConfigName, new InjectionFactory(container => GetLoggerFactory(GetAbsolutePath(nlogConfigName))));
            Container.RegisterType<IExternalSystemNotificationService, ExternalSystemNotificationService>(serviceName);

            var assemblyType = typeof(SystemNotificationProcessor);
            Container.RegisterType<SystemNotificationProcessor>(
                assemblyType.Name,
                new TransientLifetimeManager(),
                new InjectionFactory(c => new SystemNotificationProcessor(
                        Container.Resolve<IPrtgNLogFactory>(nlogConfigName).GetLogger(assemblyType),
                        new PrtgErrorsHandler(Container.Resolve<IPrtgNLogFactory>(nlogConfigName)),
                        c.Resolve<ISchedulerCustomers>(),
                        c.Resolve<IExternalSystemNotificationService>(serviceName),
                        c.Resolve<INotificationProvider>()
                    )
                ));

            RegisterSystemCleanupProcessor($"{serviceName}.Cleanup");
        }

        private void RegisterSystemCleanupProcessor(string serviceName)
        {
            var nlogConfigName = $"NLog.{serviceName}.config";
            Container.RegisterProcessor<SystemCleanupProcessor>(ServiceName, serviceName);
            Container.RegisterType<IPrtgNLogFactory>(nlogConfigName, new InjectionFactory(container => GetLoggerFactory(GetAbsolutePath(nlogConfigName))));
            Container.RegisterType<IExternalSystemNotificationService, ExternalSystemNotificationService>(serviceName);

            var assemblyType = typeof(SystemCleanupProcessor);
            Container.RegisterType<SystemCleanupProcessor>(
                assemblyType.Name,
                new TransientLifetimeManager(),
                new InjectionFactory(c => new SystemCleanupProcessor(
                        Container.Resolve<IPrtgNLogFactory>(nlogConfigName).GetLogger(assemblyType),
                        new PrtgErrorsHandler(Container.Resolve<IPrtgNLogFactory>(nlogConfigName)),
                        c.Resolve<ISchedulerCustomers>(),
                        c.Resolve<IExternalSystemNotificationService>(serviceName)
                    )
                ));
        }

        private void RegisterInterfaceNotificationsProcessor(string serviceName)
        {
            var nlogConfigName = $"NLog.{serviceName}.config";
            Container.RegisterProcessor<InterfaceNotificationProcessor>(ServiceName, serviceName);
            Container.RegisterType<IPrtgNLogFactory>(nlogConfigName, new InjectionFactory(container => GetLoggerFactory(GetAbsolutePath(nlogConfigName))));
            Container.RegisterType<IExternalInterfaceNotificationService, ExternalInterfaceNotificationService>(serviceName);

            var assemblyType = typeof(InterfaceNotificationProcessor);
            Container.RegisterType<InterfaceNotificationProcessor>(
                assemblyType.Name,
                new TransientLifetimeManager(),
                new InjectionFactory(c => new InterfaceNotificationProcessor(
                        Container.Resolve<IPrtgNLogFactory>(nlogConfigName).GetLogger(assemblyType),
                        new PrtgErrorsHandler(Container.Resolve<IPrtgNLogFactory>(nlogConfigName)),
                        c.Resolve<ISchedulerCustomers>(),
                        c.Resolve<IExternalInterfaceNotificationService>(serviceName),
                        c.Resolve<INotificationProvider>()
                    )
                ));

            RegisterInterfaceCleanupProcessor($"{serviceName}.Cleanup");
        }

        private void RegisterInterfaceCleanupProcessor(string serviceName)
        {
            var nlogConfigName = $"NLog.{serviceName}.config";
            Container.RegisterProcessor<InterfaceCleanupProcessor>(ServiceName, serviceName);
            Container.RegisterType<IPrtgNLogFactory>(nlogConfigName, new InjectionFactory(container => GetLoggerFactory(GetAbsolutePath(nlogConfigName))));
            Container.RegisterType<IExternalInterfaceNotificationService, ExternalInterfaceNotificationService>(serviceName);

            var assemblyType = typeof(InterfaceCleanupProcessor);
            Container.RegisterType<InterfaceCleanupProcessor>(
                assemblyType.Name,
                new TransientLifetimeManager(),
                new InjectionFactory(c => new InterfaceCleanupProcessor(
                        Container.Resolve<IPrtgNLogFactory>(nlogConfigName).GetLogger(assemblyType),
                        new PrtgErrorsHandler(Container.Resolve<IPrtgNLogFactory>(nlogConfigName)),
                        c.Resolve<ISchedulerCustomers>(),
                        c.Resolve<IExternalInterfaceNotificationService>(serviceName)
                    )
                ));
        }

        private static IPrtgNLogFactory GetLoggerFactory(string configPath)
        {
            return new PrtgNLogFactory(
                configPath,
                LoggerData.DefaultPrtgServiceStateVariableName,
                LoggerData.DefaultPrtgServiceQueueVariableName,
                LoggerData.DefaultPrtgServiceStatusVariableName
            );
        }

        private static string GetAbsolutePath(string relativePath)
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath);
        }
    }
}
