using Microsoft.Practices.Unity;
using Quantumart.QP8.Scheduler.API;
using System;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;

namespace Quantumart.QP8.Scheduler.Service
{
	[RunInstaller(true)]
	public class ProjectInstaller : Installer
	{
		private readonly TraceSource _logger;

		public ProjectInstaller()
		{
			//Debugger.Launch();
			_logger = GetLogger();

			try
			{
				using (var container = new UnityContainer())
				{
					container.AddNewExtension<ServiceConfiguration>();

					var processInstaller = new ServiceProcessInstaller();
					processInstaller.Account = ServiceAccount.LocalSystem;

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
					Installers.AddRange(serviceInstallers.ToArray());

					var names = descriptors.Select(d => d.Key).ToArray();
					_logger.TraceInformation("ServiceInstallers: {0}", string.Join(", ", names));
				}
			}
			catch (Exception ex)
			{
				_logger.TraceData(TraceEventType.Error, EventIdentificators.Common, ex);
				throw;
			}			
		}

		protected override void OnAfterInstall(System.Collections.IDictionary savedState)
		{
			_logger.TraceInformation("Version {0} has been installed", GetVersion());
			base.OnAfterInstall(savedState);
		}

		protected override void OnAfterRollback(System.Collections.IDictionary savedState)
		{
			_logger.TraceInformation("Rollback for version {0}", GetVersion());
			base.OnAfterRollback(savedState);
		}

		protected override void OnAfterUninstall(System.Collections.IDictionary savedState)
		{
			_logger.TraceInformation("Version {0} has been uninstalled", GetVersion());
			base.OnAfterUninstall(savedState);
		}

		private TraceSource GetLogger()
		{
			Trace.AutoFlush = true;
			var assembly = GetType().Assembly;
			var title = assembly.GetName().Name;
			var logger = new TraceSource(title);
			var fileName = Path.Combine(Path.GetDirectoryName(assembly.Location), title + ".log");
			var listener = new TextWriterTraceListener(fileName) { TraceOutputOptions = TraceOptions.DateTime };
			logger.Listeners.Add(listener);
			logger.Switch.Level = SourceLevels.All;
			return logger;
		}

		private Version GetVersion()
		{
			return GetType().Assembly.GetName().Version;
		}

		protected override void Dispose(bool disposing)
		{
			_logger.Flush();
			_logger.Close();
			base.Dispose(disposing);
		}
	}
}