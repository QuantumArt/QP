using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            Container.RegisterType<IConnectionStrings, ConnectionStrings>();

            var descriptors = Container.ResolveAll<ProcessorDescriptor>();
            var services = descriptors.Select(d => d.Service).Distinct();
            foreach (var service in services)
            {
                Container.RegisterType<Func<IUnityContainer>>(service, new InjectionFactory(parent =>
                {
                    Func<IUnityContainer> factory = () =>
                    {
                        var container = parent.CreateChildContainer();

                        container.LoadConfiguration();
                        container.RegisterType<TraceSource>(new InjectionFactory(c => c.Resolve<TraceSource>(service)));
                        container.RegisterType<ServiceDescriptor>(new InjectionFactory(c => c.Resolve<ServiceDescriptor>(service)));
                        container.RegisterType<IScheduler, Scheduler>(new HierarchicalLifetimeManager(), new InjectionFactory(c => new Scheduler(c.Resolve<IEnumerable<IProcessor>>(), c.Resolve<TraceSource>())));
                        Container.RegisterType<IEnumerable<IProcessor>>(new InjectionFactory(c =>
                            from d in descriptors
                            where d.Service == service
                            select new ScheduledProcessor(c.Resolve<Func<IProcessor>>(d.Processor), c.Resolve<Func<ISchedule>>(d.Schedule))
                        ));

                        return container;
                    };

                    return factory;
                }));
            }
        }
    }
}
