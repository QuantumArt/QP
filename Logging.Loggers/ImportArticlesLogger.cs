using System.Diagnostics;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using Quantumart.QP8.BLL.Services.MultistepActions.Import;

namespace Quantumart.QP8.Logging.Loggers
{
    public class ImportArticlesLogger : LoggerBase, IImportArticlesLogger
    {
        public ImportArticlesLogger(LogWriter logWriter, ExceptionManager exceptionManager)
            : base(logWriter, exceptionManager)
        {
        }

        public void LogStartImport(ImportSettings settings)
        {
            Log("Start import articles " + settings.Id);
        }

        public void LogStep(int step, ImportSettings settings)
        {
            var model = new ImportStepSettings(step, settings);
            Log("Import articles, step " + step, model);
        }

        public void LogEndImport(ImportSettings settings)
        {
            Log("End import articles", settings);
        }

        private void Log(string message, object settings)
        {
            var context = new LoggerContext();
            Log(message, TraceEventType.Verbose, settings, context);
        }

        private void Log(string message)
        {
            var context = new LoggerContext();
            Log(message, TraceEventType.Verbose, context);
        }
    }
}
