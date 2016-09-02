using System.Diagnostics;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using Microsoft.Practices.EnterpriseLibrary.Logging.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Logging.TraceListeners;
using Quantumart.QP8.Logging.Formatters;
using Quantumart.QP8.Logging.Web.Repository;

namespace Quantumart.QP8.Logging.Web.Listeners
{
    [ConfigurationElementType(typeof(CustomTraceListenerData))]
    public class HttpContextTraceListener : CustomTraceListener
    {
        public override void Write(string message)
        {
            HttpContextRepository.Add(message, Name);
        }

        public override void WriteLine(string message)
        {
            HttpContextRepository.Add(message, Name);
        }

        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, object data)
        {
            var logEntry = data as LogEntry;
            if (logEntry != null && Formatter != null)
            {
                var templateFormatter = Formatter as TemplateFormatter;
                WriteLine(templateFormatter == null ? Formatter.Format(logEntry) : templateFormatter.Format(logEntry, TraceOutputOptions));
            }
        }
    }
}
