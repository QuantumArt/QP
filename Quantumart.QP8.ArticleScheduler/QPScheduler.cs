using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Unity;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using Quantumart.QP8.Configuration;

namespace Quantumart.QP8.ArticleScheduler
{
	/// <summary>
	/// Класс выполняет задачи из всех БД
	/// </summary>
	public class QPScheduler
	{
		IEnumerable<string> connectionStrings;
		IUnityContainer unityContainer;

		public QPScheduler(IEnumerable<string> connectionStrings, IUnityContainer unityContainer)
		{
			if (unityContainer == null)
				throw new ArgumentNullException("unityContainer");

			this.connectionStrings = connectionStrings;
			this.unityContainer = unityContainer;
		}

		/// <summary>
		/// Выполняет расписания
		/// </summary>
		public void ParallelRun()
		{							
			connectionStrings.AsParallel().ForAll(cs =>
			{
				try
				{
					new DbScheduler(cs, unityContainer).ParallelRun();
				}				
				catch (Exception exp)
				{
					UnityContainerCustomizer.UnityContainer.Resolve<IExceptionHandler>().HandleException(exp);
				}
			});			
		}		
	}
}
