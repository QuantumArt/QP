using System;
using QP8.Infrastructure.Logging;
using QP8.Infrastructure.Logging.Factories;

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

            LogProvider.GetLogger("prtg").Error("There was an error at article sheduler service.");
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
