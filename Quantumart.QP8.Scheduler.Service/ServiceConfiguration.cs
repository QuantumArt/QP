using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using Quantumart.QP8.Scheduler.API;
using Quantumart.QP8.Scheduler.Core;
using Quantumart.QP8.Scheduler.Users;
using System.ServiceProcess;

using System.Linq;
using System.Diagnostics;
using System;
using Quantumart.QP8.Scheduler.Notification;

namespace Quantumart.QP8.Scheduler.Service
{
	internal class ServiceConfiguration : UnityContainerExtension
	{
		protected override void Initialize()
		{
			Container.AddNewExtension<SchedulerUsersConfiguration>();
			Container.AddNewExtension<SchedulerNotificationConfiguration>();
			Container.AddNewExtension<SchedulerCoreConfiguration>();

			var descriptors = Container.ResolveAll<ServiceDescriptor>();

			foreach (var descriptor in descriptors)
			{
				Container.RegisterType<ServiceBase, SchedulerService>(descriptor.Name, new InjectionFactory(c => new SchedulerService(c.Resolve<Func<IUnityContainer>>(descriptor.Key), descriptor)));
			}			
		}
	}
}