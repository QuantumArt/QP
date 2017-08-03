using System.IO;
using System.Reflection;
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
            Container.RegisterType<IInterfaceNotificationProvider, InterfaceNotificationProvider>(new ContainerControlledLifetimeManager());

            RegisterSystemNotificationsProcessor("System.Notifications");
            RegisterInterfaceNotificationsProcessor("Interface.Notifications");

            Mapper.AddProfile<NotificationMapperProfile>();
        }

        private void RegisterSystemNotificationsProcessor(string serviceConfigName)
        {
            var nlogConfigName = $"NLog.{serviceConfigName}.config";

            Container.RegisterProcessor<SystemNotificationProcessor>(ServiceName, serviceConfigName);
            Container.RegisterType<IPrtgNLogFactory>(nlogConfigName, new ContainerControlledLifetimeManager(), new InjectionFactory(container => GetLoggerFactory(GetAbsolutePath(nlogConfigName))));
            Container.RegisterType<IExternalSystemNotificationService, ExternalSystemNotificationService>(serviceConfigName, new ContainerControlledLifetimeManager());

            var assemblyType = typeof(SystemNotificationProcessor);
            var logger = Container.Resolve<IPrtgNLogFactory>(nlogConfigName).GetLogger(assemblyType);
            var prtgErrorsHandler = new PrtgErrorsHandler(Container.Resolve<IPrtgNLogFactory>(nlogConfigName));

            Container.RegisterType<SystemNotificationProcessor>(
                assemblyType.Name,
                new TransientLifetimeManager(),
                new InjectionFactory(c => new SystemNotificationProcessor(
                    logger,
                    prtgErrorsHandler,
                    c.Resolve<ISchedulerCustomerCollection>(),
                    c.Resolve<IExternalSystemNotificationService>(serviceConfigName))
                ));

            RegisterSystemCleanupProcessor($"{serviceConfigName}.Cleanup");
        }

        private void RegisterSystemCleanupProcessor(string serviceConfigName)
        {
            var nlogConfigName = $"NLog.{serviceConfigName}.config";
            Container.RegisterProcessor<SystemCleanupProcessor>(ServiceName, serviceConfigName);
            Container.RegisterType<IPrtgNLogFactory>(nlogConfigName, new ContainerControlledLifetimeManager(), new InjectionFactory(container => GetLoggerFactory(GetAbsolutePath(nlogConfigName))));
            Container.RegisterType<IExternalSystemNotificationService, ExternalSystemNotificationService>(serviceConfigName, new ContainerControlledLifetimeManager());

            var assemblyType = typeof(SystemCleanupProcessor);
            var logger = Container.Resolve<IPrtgNLogFactory>(nlogConfigName).GetLogger(assemblyType);
            var prtgErrorsHandler = new PrtgErrorsHandler(Container.Resolve<IPrtgNLogFactory>(nlogConfigName));

            Container.RegisterType<SystemCleanupProcessor>(
                assemblyType.Name,
                new TransientLifetimeManager(),
                new InjectionFactory(c => new SystemCleanupProcessor(
                    logger,
                    prtgErrorsHandler,
                    c.Resolve<ISchedulerCustomerCollection>(),
                    c.Resolve<IExternalSystemNotificationService>(serviceConfigName))
                ));
        }

        private void RegisterInterfaceNotificationsProcessor(string serviceConfigName)
        {
            var nlogConfigName = $"NLog.{serviceConfigName}.config";
            Container.RegisterProcessor<InterfaceNotificationProcessor>(ServiceName, serviceConfigName);
            Container.RegisterType<IPrtgNLogFactory>(nlogConfigName, new ContainerControlledLifetimeManager(), new InjectionFactory(container => GetLoggerFactory(GetAbsolutePath(nlogConfigName))));
            Container.RegisterType<IExternalInterfaceNotificationService, ExternalInterfaceNotificationService>(serviceConfigName, new ContainerControlledLifetimeManager());

            var assemblyType = typeof(InterfaceNotificationProcessor);
            var logger = Container.Resolve<IPrtgNLogFactory>(nlogConfigName).GetLogger(assemblyType);
            var prtgErrorsHandler = new PrtgErrorsHandler(Container.Resolve<IPrtgNLogFactory>(nlogConfigName));

            Container.RegisterType<InterfaceNotificationProcessor>(
                assemblyType.Name,
                new TransientLifetimeManager(),
                new InjectionFactory(c => new InterfaceNotificationProcessor(
                    logger,
                    prtgErrorsHandler,
                    c.Resolve<ISchedulerCustomerCollection>(),
                    c.Resolve<IExternalInterfaceNotificationService>(serviceConfigName),
                    c.Resolve<IInterfaceNotificationProvider>())
                ));

            RegisterInterfaceCleanupProcessor($"{serviceConfigName}.Cleanup");
        }

        private void RegisterInterfaceCleanupProcessor(string serviceConfigName)
        {
            var nlogConfigName = $"NLog.{serviceConfigName}.config";
            Container.RegisterProcessor<InterfaceCleanupProcessor>(ServiceName, serviceConfigName);
            Container.RegisterType<IPrtgNLogFactory>(nlogConfigName, new ContainerControlledLifetimeManager(), new InjectionFactory(container => GetLoggerFactory(GetAbsolutePath(nlogConfigName))));
            Container.RegisterType<IExternalInterfaceNotificationService, ExternalInterfaceNotificationService>(serviceConfigName, new ContainerControlledLifetimeManager());

            var assemblyType = typeof(InterfaceCleanupProcessor);
            var logger = Container.Resolve<IPrtgNLogFactory>(nlogConfigName).GetLogger(assemblyType);
            var prtgErrorsHandler = new PrtgErrorsHandler(Container.Resolve<IPrtgNLogFactory>(nlogConfigName));

            Container.RegisterType<InterfaceCleanupProcessor>(
                assemblyType.Name,
                new TransientLifetimeManager(),
                new InjectionFactory(c => new InterfaceCleanupProcessor(
                    logger,
                    prtgErrorsHandler,
                    c.Resolve<ISchedulerCustomerCollection>(),
                    c.Resolve<IExternalInterfaceNotificationService>(serviceConfigName))
                ));
        }

        private static IPrtgNLogFactory GetLoggerFactory(string configPath) => new PrtgNLogFactory(
            configPath,
            LoggerData.DefaultPrtgServiceStateVariableName,
            LoggerData.DefaultPrtgServiceQueueVariableName,
            LoggerData.DefaultPrtgServiceStatusVariableName
        );

        private static string GetAbsolutePath(string relativePath) => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), relativePath);
    }
}
