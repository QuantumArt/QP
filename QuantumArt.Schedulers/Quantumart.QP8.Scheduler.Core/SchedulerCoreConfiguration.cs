using System;
using System.Collections.Generic;
using System.Linq;
using Quantumart.QP8.Scheduler.API;
using Unity;
using Unity.Extension;
using Unity.Injection;
using Unity.Lifetime;

namespace Quantumart.QP8.Scheduler.Core
{
    public class SchedulerCoreConfiguration : UnityContainerExtension
    {
        protected override void Initialize()
        {
            Container.RegisterType<ISchedule, NullSchedule>(ConfigurationExtension.DefaultScheduleName);
            Container.RegisterType<ISchedulerCustomerCollection, SchedulerCustomerCollection>();

            var descriptors = Container.ResolveAll<ProcessorDescriptor>().ToList();
            var services = descriptors.Select(d => d.Service).Distinct().ToList();
            foreach (var service in services)
            {
                Container.RegisterType<Func<IUnityContainer>>(service, new InjectionFactory(parent =>
                {
                    IUnityContainer Factory()
                    {
                        var container = parent.CreateChildContainer();
                        container.RegisterType<ISchedule, IntervalSchedule>("UserSynchronizationSchedule", new HierarchicalLifetimeManager(), new InjectionConstructor(TimeSpan.Parse("00:00:30")));
                        container.RegisterType<ISchedule, IntervalSchedule>("System.Notifications", new HierarchicalLifetimeManager(), new InjectionConstructor(TimeSpan.Parse("00:00:30")));
                        container.RegisterType<ISchedule, IntervalSchedule>("Interface.Notifications", new HierarchicalLifetimeManager(), new InjectionConstructor(TimeSpan.Parse("00:00:30")));
                        container.RegisterType<ISchedule, IntervalSchedule>("System.Notifications.Cleanup", new HierarchicalLifetimeManager(), new InjectionConstructor(TimeSpan.Parse("00:05:00")));
                        container.RegisterType<ISchedule, IntervalSchedule>("Interface.Notifications.Cleanup", new HierarchicalLifetimeManager(), new InjectionConstructor(TimeSpan.Parse("00:05:00")));

                        container.RegisterType<ServiceDescriptor>(new InjectionFactory(c => c.Resolve<ServiceDescriptor>(service)));
                        container.RegisterType<IScheduler, Scheduler>(new HierarchicalLifetimeManager(), new InjectionFactory(c => new Scheduler(c.Resolve<IEnumerable<IProcessor>>())));
                        Container.RegisterType<IEnumerable<IProcessor>>(new InjectionFactory(c => descriptors.Where(d => d.Service == service).Select(d => new ScheduledProcessor(c.Resolve<Func<IProcessor>>(d.Processor), c.Resolve<Func<ISchedule>>(d.Schedule)))));

                        return container;
                    }

                    return (Func<IUnityContainer>)Factory;
                }));
            }
        }
    }
}
