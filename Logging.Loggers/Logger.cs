using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using System.Diagnostics;

namespace Quantumart.QP8.Logging.Loggers
{
	public class Logger<T> : LoggerBase, ILogger<T> where T : class
	{
		public Logger(LogWriter logWriter, ExceptionManager exceptionManager)
			: base(logWriter, exceptionManager)
		{
		}

		public void Log(T model)
		{
			var context = new LoggerContext();
			Log(GetName(), TraceEventType.Verbose, model, context);
		}
	}
}
