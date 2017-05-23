using System;
using System.IO;
using Microsoft.Practices.Unity;
using QP8.Infrastructure.Logging.Factories;
using QP8.Infrastructure.Logging.PrtgMonitoring.NLogExtensions.Factories;
using QP8.Infrastructure.Logging.PrtgMonitoring.NLogExtensions.Interfaces;
using Quantumart.QP8.BLL.Logging;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.BLL.Services.UserSynchronization;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Scheduler.API;

namespace Quantumart.QP8.Scheduler.Users
{
    public class SchedulerUsersConfiguration : UnityContainerExtension
    {
        public const string ServiceName = "qp8.users";
        private const string UsersNlogConfigName = "NLog.Users.config";

        protected override void Initialize()
        {
            Container.RegisterService(ServiceName, "QP8 Users Synchronization", "Синхронизация пользователей QP8 с Active Directory");

            Container.RegisterType<IUserService, UserService>();
            Container.RegisterType<IUserSynchronizationService, UserSynchronizationService>(new InjectionFactory(c => UserSynchronizationServiceFactory.GetService(LogProvider.GetLogger())));

            Container.RegisterProcessor<UsersProcessor>(ServiceName, "UserSynchronizationSchedule");
            RegisterUsersProcessor();
        }

        private void RegisterUsersProcessor()
        {
            var assemblyType = typeof(UsersProcessor);
            var nlogInjectionFactory = new InjectionFactory(container => GetLoggerFactory(GetAbsolutePath(UsersNlogConfigName)));
            Container.RegisterType<IPrtgNLogFactory>(UsersNlogConfigName, nlogInjectionFactory);
            Container.RegisterType<IProcessor, UsersProcessor>(
                assemblyType.Name,
                new TransientLifetimeManager(),
                new InjectionFactory(c => new UsersProcessor(
                        Container.Resolve<IPrtgNLogFactory>(UsersNlogConfigName).GetLogger(assemblyType),
                        new PrtgErrorsHandler(Container.Resolve<IPrtgNLogFactory>(UsersNlogConfigName)),
                        c.Resolve<ISchedulerCustomerCollection>(),
                        c.Resolve<Func<IUserSynchronizationService>>()
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
