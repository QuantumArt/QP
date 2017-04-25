using System;
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

        protected override void Initialize()
        {
            Container.RegisterService(ServiceName, "QP8 Users Synchronization", "Синхронизация пользователей QP8 с Active Directory");

            Container.RegisterType<IUserService, UserService>();
            Container.RegisterType<IUserSynchronizationService, UserSynchronizationService>(new InjectionFactory(c => UserSynchronizationServiceFactory.GetService(LogProvider.GetLogger())));

            Container.RegisterProcessor<UsersProcessor>(ServiceName, "CleanupNotificationQueueSchedule");
            RegisterCleanupProcessor();
        }


        private void RegisterCleanupProcessor()
        {
            var childContainer = Container.CreateChildContainer();
            childContainer.RegisterType<IPrtgNLogFactory>(new InjectionFactory(container => GetLoggerFactory("NLog.Users.config")));

            var assemblyType = typeof(UsersProcessor);
            Container.RegisterType<IProcessor, UsersProcessor>(
                assemblyType.Name,
                new TransientLifetimeManager(),
                new InjectionFactory(c => new UsersProcessor(
                    childContainer.Resolve<IPrtgNLogFactory>().GetLogger(assemblyType),
                    new PrtgErrorsHandler(childContainer.Resolve<IPrtgNLogFactory>()),
                    c.Resolve<ISchedulerCustomers>(),
                    c.Resolve<Func<IUserSynchronizationService>>()
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
    }
}
