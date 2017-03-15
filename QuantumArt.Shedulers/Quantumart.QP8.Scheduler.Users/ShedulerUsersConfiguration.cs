using System;
using System.Diagnostics;
using Microsoft.Practices.Unity;
using QP8.Infrastructure.Logging.Factories;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.BLL.Services.UserSynchronization;
using Quantumart.QP8.Scheduler.API;

namespace Quantumart.QP8.Scheduler.Users
{
    public class SchedulerUsersConfiguration : UnityContainerExtension
    {
        public const string UsersService = "qp8.users";

        protected override void Initialize()
        {
            Container.RegisterType<IUserService, UserService>();
            Container.RegisterService(UsersService, "QP8 Users Synchronization", "Синхронизация пользователей QP8 с Active Directory");

            var assemblyType = typeof(UsersProcessor);
            Container.RegisterType<IProcessor, UsersProcessor>(
                assemblyType.Name,
                new TransientLifetimeManager(),
                new InjectionFactory(c => new UsersProcessor(
                    LogProvider.LogFactory.GetLogger(assemblyType),
                    c.Resolve<IConnectionStrings>(),
                    c.Resolve<Func<IUserSynchronizationService>>()
               )
           ));

            var descriptor = new ProcessorDescriptor(assemblyType.Name, UsersService, "UserSynchronizationSchedule");
            Container.RegisterInstance(descriptor.Processor, descriptor);

            Container.RegisterType<IUserSynchronizationService, UserSynchronizationService>(new InjectionFactory(c => UserSynchronizationServiceFactory.GetService(c.Resolve<TraceSource>())));
        }
    }
}
