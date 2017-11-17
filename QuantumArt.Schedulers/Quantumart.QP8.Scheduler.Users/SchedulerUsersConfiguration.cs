using System;
using System.IO;
using System.Reflection;
using QP8.Infrastructure.Logging.Factories;
using QP8.Infrastructure.Logging.PrtgMonitoring.NLogExtensions.Factories;
using QP8.Infrastructure.Logging.PrtgMonitoring.NLogExtensions.Interfaces;
using Quantumart.QP8.BLL.Logging;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.BLL.Services.UserSynchronization;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Scheduler.API;
using Unity;
using Unity.Extension;
using Unity.Injection;
using Unity.Lifetime;

namespace Quantumart.QP8.Scheduler.Users
{
    public class SchedulerUsersConfiguration : UnityContainerExtension
    {
        public const string ServiceName = "qp8.users";
        private const string UsersNlogConfigName = "NLog.Users.config";

        protected override void Initialize()
        {
            Container.RegisterService(ServiceName, "QP8 Users Synchronization", "Синхронизация пользователей QP8 с Active Directory");

            Container.RegisterType<IUserService, UserService>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IUserSynchronizationService, UserSynchronizationService>(new InjectionFactory(c => UserSynchronizationServiceFactory.GetService(LogProvider.GetLogger())));

            Container.RegisterProcessor<UsersProcessor>(ServiceName, "UserSynchronizationSchedule");
            RegisterUsersProcessor();
        }

        private void RegisterUsersProcessor()
        {
            var nlogInjectionFactory = new InjectionFactory(container => GetLoggerFactory(GetAbsolutePath(UsersNlogConfigName)));
            Container.RegisterType<IPrtgNLogFactory>(UsersNlogConfigName, new ContainerControlledLifetimeManager(), nlogInjectionFactory);

            var assemblyType = typeof(UsersProcessor);
            var logger = Container.Resolve<IPrtgNLogFactory>(UsersNlogConfigName).GetLogger(assemblyType);
            var prtgErrorsHandler = new PrtgErrorsHandler(Container.Resolve<IPrtgNLogFactory>(UsersNlogConfigName));

            Container.RegisterType<IProcessor, UsersProcessor>(
                assemblyType.Name,
                new TransientLifetimeManager(),
                new InjectionFactory(c => new UsersProcessor(
                    logger,
                    prtgErrorsHandler,
                    c.Resolve<ISchedulerCustomerCollection>(),
                    c.Resolve<Func<IUserSynchronizationService>>())
                ));
        }

        private static IPrtgNLogFactory GetLoggerFactory(string configPath) => new PrtgNLogFactory(
            configPath,
            LoggerData.DefaultPrtgServiceStateVariableName,
            LoggerData.DefaultPrtgServiceQueueVariableName,
            LoggerData.DefaultPrtgServiceStatusVariableName
        );

        private static string GetAbsolutePath(string relativePath) => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? throw new InvalidOperationException(), relativePath);
    }
}
