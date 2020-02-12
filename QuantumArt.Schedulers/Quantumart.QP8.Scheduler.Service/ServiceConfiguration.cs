using System;
using System.ServiceProcess;
using AutoMapper;
using QP8.Infrastructure.Logging.Unity;
using Quantumart.QP8.BLL.Facades;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Scheduler.API;
using Quantumart.QP8.Scheduler.Core;
using Quantumart.QP8.Scheduler.Notification;
using Quantumart.QP8.Scheduler.Users;
using Unity;
using Unity.Extension;
using Unity.Injection;

namespace Quantumart.QP8.Scheduler.Service
{
    internal class ServiceConfiguration : UnityContainerExtension
    {
        protected override void Initialize()
        {
            Mapper.Initialize(cfg =>
            {
                MapperFacade.CreateAllMappings(cfg);
                cfg.AddProfile<NotificationMapperProfile>();
            });

            Container
                .AddExtension(new NLogUnityContainerExtension())
                .AddNewExtension<SchedulerUsersConfiguration>()
                .AddNewExtension<SchedulerNotificationConfiguration>()
                .AddNewExtension<SchedulerCoreConfiguration>();

            var descriptors = Container.ResolveAll<ServiceDescriptor>();
            foreach (var descriptor in descriptors)
            {
                Container.RegisterType<ServiceBase>(descriptor.Name, new InjectionFactory(c => new SchedulerService(c.Resolve<Func<IUnityContainer>>(descriptor.Key), descriptor)));
            }
        }
    }
}
