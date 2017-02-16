using System;
using Quantumart.QP8.BLL.Factories.Logging;
using Quantumart.QP8.BLL.Services;

namespace Quantumart.QP8.ArticleScheduler
{
    internal interface IExceptionHandler
    {
        void HandleException(Exception exp);
    }

    internal class ExceptionHandler : IExceptionHandler
    {
        public void HandleException(Exception ex)
        {
            var aggregatedException = ex as AggregateException;
            if (aggregatedException != null)
            {
                HandleAggregateException(aggregatedException);
            }
            else
            {
                Logger.Log.Error(ex);
            }

            LogProvider.GetLogger("prtg").Info("PRTG Error.");
        }

        private static void HandleAggregateException(AggregateException aggregatedException)
        {
            foreach (var ex in aggregatedException.Flatten().InnerExceptions)
            {
                Logger.Log.Error(ex);
            }
        }
    }
}
