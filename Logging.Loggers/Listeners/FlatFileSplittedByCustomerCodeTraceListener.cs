using System.Diagnostics;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using Microsoft.Practices.EnterpriseLibrary.Logging.Formatters;
using Quantumart.QP8.Logging.Listeners;

namespace Quantumart.QP8.Logging.Loggers.Listeners
{
	[ConfigurationElementType(typeof(FlatFileSplittedByCustomerCodeTraceListenerData))]
	public class FlatFileSplittedByCustomerCodeTraceListener : SplittedFlatFileTraceListener
	{
		private const string CustomerCodeMark = "{CustomerCode}";

		public FlatFileSplittedByCustomerCodeTraceListener(string fileName, string header, string footer, ILogFormatter formatter, TraceOptions traceOutputOptions)
			: base(fileName, header, footer, formatter, traceOutputOptions)
		{
		}

		protected override string GetCurrentFileName(object data)
		{
			string customerCode = string.Empty;
			var logentry = data as LogEntry;

			if (logentry != null)
			{
				LoggerContext loggerContext = null;

				if (logentry.Severity == TraceEventType.Error)
				{
					loggerContext = new LoggerContext();
				}
				else
				{
					var modelLogentry = logentry as ModelLogEntry;
					if (modelLogentry != null)
					{
						loggerContext = modelLogentry.Context as LoggerContext;
					}
				}

				if (loggerContext != null)
				{
					customerCode = loggerContext.CustomerCode;
				}
			}

			return this.FileName.Replace(CustomerCodeMark, customerCode);
		}
	}
}
