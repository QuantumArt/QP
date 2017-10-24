using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;
using Quantumart.QP8.Scheduler.API;
using Quantumart.QP8.Scheduler.Core;
using Unity;

namespace Quantumart.QP8.Scheduler.Service
{
    internal static class Program
    {
        private static void Main()
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
                var isCanceled = false;
                var locker = new object();
                var runActions = new List<Action>();
                var cancelActions = new List<Action>();
                var serviceContainers = new List<IUnityContainer>();

                foreach (var service in services)
                {
                    var serviceContainer = container.Resolve<Func<IUnityContainer>>(service)();
                    var scheduler = serviceContainer.Resolve<IScheduler>();
                    serviceContainers.Add(serviceContainer);
                    runActions.Add(scheduler.Start);
                    cancelActions.Add(scheduler.Stop);
                }

                void Stop()
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
                }

                Console.CancelKeyPress += (s, e) =>
                {
                    Stop();
                    e.Cancel = true;
                };

                Parallel.Invoke(runActions.ToArray());
                Console.ReadLine();
                Stop();
            }
        }

        private static void RunService()
        {
            var container = new UnityContainer();
            container.AddNewExtension<ServiceConfiguration>();
            ServiceBase.Run(container.Resolve<ServiceBase[]>());
        }
    }
}
