using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;

namespace Quantumart.QP8.ArticleScheduler.WinService
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		static void Main()
		{
			ServiceBase[] ServicesToRun;
			ServicesToRun = new ServiceBase[] 
			{ 
			    new ArticleSchedulerService() 
			};
			ServiceBase.Run(ServicesToRun);
		}
	}
}
