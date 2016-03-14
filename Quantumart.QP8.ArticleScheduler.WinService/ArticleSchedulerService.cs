using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace Quantumart.QP8.ArticleScheduler.WinService
{
	public partial class ArticleSchedulerService : ServiceBase
	{
		QPSchedulerProcessor processor;

		public ArticleSchedulerService()
		{
			InitializeComponent();
			var settings = Properties.Settings.Default;
			processor = new QPSchedulerProcessor(settings.RecurrentTimeout, Properties.Settings.SplitExceptCustomerCodes(settings.ExceptCustomerCodes));
		}

		protected override void OnStart(string[] args)
		{
			processor.Run();
		}

		protected override void OnStop()
		{
			processor.Stop();
		}
	}
}
