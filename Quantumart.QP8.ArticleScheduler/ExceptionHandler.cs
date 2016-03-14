using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;

namespace Quantumart.QP8.ArticleScheduler
{
	interface IExceptionHandler
	{
		void HandleException(Exception exp);		
	}

	class ExceptionHandler : IExceptionHandler
	{
		#region IExceptionHandler Members

		public void HandleException(Exception exp)
		{
			if (exp is AggregateException)
				HandleAggregateException(exp as AggregateException);
			else
				EnterpriseLibraryContainer.Current
								.GetInstance<ExceptionManager>()
								.HandleException(exp, "Policy");
		}

		private void HandleAggregateException(AggregateException aexp)
		{
			var exceptionManager = EnterpriseLibraryContainer.Current.GetInstance<ExceptionManager>();
			foreach (var exp in aexp.Flatten().InnerExceptions)
			{
				exceptionManager.HandleException(exp, "Policy");
			}
		}

		#endregion
	}

}
