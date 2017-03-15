using System;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.ServiceProcess;
using Microsoft.Practices.Unity;
using QP8.Infrastructure.Logging;
using Quantumart.QP8.Scheduler.API;

namespace Quantumart.QP8.Scheduler.Service
{
    [RunInstaller(true)]
    public class ProjectInstaller : Installer
    {
        public ProjectInstaller()
        {
            try
            {
                using (var container = new UnityContainer())
                {
                    container.AddNewExtension<ServiceConfiguration>();
                    var processInstaller = new ServiceProcessInstaller { Account = ServiceAccount.LocalSystem };
                    var descriptors = container.Resolve<ServiceDescriptor[]>();
                    var serviceInstallers = from descriptor in descriptors
                                            select new ServiceInstaller
                                            {
                                                StartType = ServiceStartMode.Manual,
                                                DisplayName = descriptor.Name,
                                                ServiceName = descriptor.Key,
                                                Description = descriptor.Description
                                            };

                    Installers.Add(processInstaller);
                    Installers.AddRange(serviceInstallers.ToArray<Installer>());

                    Logger.Log.Info($"ServiceInstallers: {string.Join(", ", descriptors.Select(d => d.Key))}");
                }
            }
            catch (Exception ex)
            {
                Logger.Log.Fatal(ex);
                throw;
            }
        }

        protected override void OnAfterInstall(System.Collections.IDictionary savedState)
        {
            Logger.Log.Info($"Version {GetVersion()} has been installed");
            base.OnAfterInstall(savedState);
        }

        protected override void OnAfterRollback(System.Collections.IDictionary savedState)
        {
            Logger.Log.Info($"Rollback for version {GetVersion()}");
            base.OnAfterRollback(savedState);
        }

        protected override void OnAfterUninstall(System.Collections.IDictionary savedState)
        {
            Logger.Log.Info($"Version {GetVersion()} has been uninstalled");
            base.OnAfterUninstall(savedState);
        }

        private Version GetVersion()
        {
            return GetType().Assembly.GetName().Version;
        }

        protected override void Dispose(bool disposing)
        {
            Logger.Log.Dispose();
            base.Dispose(disposing);
        }
    }
}
