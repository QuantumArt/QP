using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using Quantumart.QP8.Scheduler.API;

namespace Quantumart.QP8.Scheduler.Core
{
    public class SchedulerCoreConfiguration : UnityContainerExtension
    {
        protected override void Initialize()
        {
            Container.RegisterType<ISchedule, NullSchedule>(ConfigurationExtension.DefaultScheduleName);
            Container.RegisterType<ISchedulerCustomers, SchedulerCustomers>();

            var descriptors = Container.ResolveAll<ProcessorDescriptor>().ToList();
            var services = descriptors.Select(d => d.Service).Distinct().ToList();
            foreach (var service in services)
            {
                Container.RegisterType<Func<IUnityContainer>>(service, new InjectionFactory(parent =>
                {
                    IUnityContainer Factory()
                    {
                        var container = parent.CreateChildContainer();
                        container.LoadConfiguration();
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
