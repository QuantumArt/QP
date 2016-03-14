using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Quantumart.QP8.Configuration;
using Microsoft.Practices.Unity;

namespace Quantumart.QP8.ArticleScheduler
{
	/// <summary>
	/// Запускает QPScheduler в отдельном потоке 
	/// </summary>
	public class QPSchedulerProcessor
	{
		private CancellationTokenSource cancellationTokenSource;
		private Task task;

		private readonly TimeSpan recurrentTimeout;
		private readonly IEnumerable<string> exceptCustomerCodes;

		private static readonly string APP_NAME = "QP8ArticleSchedulerService";		

		public QPSchedulerProcessor(TimeSpan recurrentTimeout, IEnumerable<string> exceptCustomerCodes)
		{
			this.recurrentTimeout = recurrentTimeout;
			this.exceptCustomerCodes = exceptCustomerCodes ?? new string[0];
		}

		public void Run()
		{
			cancellationTokenSource = new CancellationTokenSource();
			task = new Task(() =>
			{

				do
				{
					try
					{
						new QPScheduler(QPConfiguration.ConfigConnectionStrings(APP_NAME, exceptCustomerCodes), UnityContainerCustomizer.UnityContainer).ParallelRun();
					}
					catch (Exception exp)
					{
						UnityContainerCustomizer.UnityContainer.Resolve<IExceptionHandler>().HandleException(exp);
					}
				}
				while (!cancellationTokenSource.Token.WaitHandle.WaitOne(recurrentTimeout));
			}, cancellationTokenSource.Token, TaskCreationOptions.LongRunning);
			task.Start();
		}

		public void Stop()
		{
			try
			{
				cancellationTokenSource.Cancel();
				task.Wait();
			}
			catch (Exception exp)
			{
				UnityContainerCustomizer.UnityContainer.Resolve<IExceptionHandler>().HandleException(exp);
			}
			finally
			{
				cancellationTokenSource.Dispose();
				task.Dispose();
				cancellationTokenSource = null;
				task = null;
			}
		}
	}
}
