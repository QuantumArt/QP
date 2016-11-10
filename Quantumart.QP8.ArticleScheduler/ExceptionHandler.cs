using System;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;

namespace Quantumart.QP8.ArticleScheduler
{
    internal interface IExceptionHandler
    {
        void HandleException(Exception exp);
    }

    internal class ExceptionHandler : IExceptionHandler
    {
        public void HandleException(Exception exp)
        {
            var agrExc = exp as AggregateException;
            if (agrExc != null)
            {
                HandleAggregateException(agrExc);
            }
            else
            {
                EnterpriseLibraryContainer.Current.GetInstance<ExceptionManager>().HandleException(exp, "Policy");
            }
        }

        private static void HandleAggregateException(AggregateException aexp)
        {
            var exceptionManager = EnterpriseLibraryContainer.Current.GetInstance<ExceptionManager>();
            foreach (var exp in aexp.Flatten().InnerExceptions)
            {
                exceptionManager.HandleException(exp, "Policy");
            }
        }
    }
}
