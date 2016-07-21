using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using Quantumart.QP8.Scheduler.API;
using Quantumart.QP8.Scheduler.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace Quantumart.QP8.Scheduler.Service
{
	static class Program
	{
		static void Main(string[] args)
		{
			if (Environment.UserInteractive)
			{
				RunConsole();
			}
			else
			{
				RunService();
			}			
		}

		private static void RunConsole()
		{
			using (var container = new UnityContainer())
			{
				container.AddNewExtension<ServiceConfiguration>();

				var descriptors = container.ResolveAll<ProcessorDescriptor>();
				var services = descriptors.Select(d => d.Service).Distinct();
				bool isCanceled = false;
				var locker = new object();
				var runActions = new List<Action>();
				var cancelActions = new List<Action>();
				var serviceContainers = new List<IUnityContainer>();

				foreach(var service in services)
				{
					var serviceContainer = container.Resolve<Func<IUnityContainer>>(service)();
					var scheduler = serviceContainer.Resolve<IScheduler>();
					serviceContainers.Add(serviceContainer);
					runActions.Add(scheduler.Start);
					cancelActions.Add(scheduler.Stop);
				}
							
				Action stop = () =>
				{
					lock (locker)
					{
						if (!isCanceled)
						{
							Parallel.Invoke(cancelActions.ToArray());
							serviceContainers.ForEach(c => c.Dispose());
							isCanceled = true;
						}
					}
				};

				Console.CancelKeyPress += (s, e) =>
				{
					stop();
					e.Cancel = true;
				};

				Parallel.Invoke(runActions.ToArray());
				Console.ReadLine();
				stop();
			}
		}

		private static void RunService()
		{
			var container = new UnityContainer();
			container.AddNewExtension<ServiceConfiguration>();
			var services = container.Resolve<ServiceBase[]>();
			ServiceBase.Run(services);
		}
	}
}