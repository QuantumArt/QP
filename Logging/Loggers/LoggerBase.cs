using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.Logging.Loggers
{
    public class LoggerBase
    {
        private readonly LogWriter _logWriter;
        private readonly ExceptionManager _exceptionManager;

        public LoggerBase(LogWriter logWriter, ExceptionManager exceptionManager)
        {
            _logWriter = logWriter;
            _exceptionManager = exceptionManager;
        }

        protected void LogException(Exception ex)
        {
            _exceptionManager.HandleException(ex, LoggingData.EnterpiseExceptionPolicyName);
        }

        protected void Log(string message, TraceEventType severity, object model, object context)
        {
            var modelCategory = model.GetType().Name;
            var loggerCategory = GetName();
            var modelLoggerCategory = loggerCategory + "/" + modelCategory;
            var logEntry = new ModelLogEntry
            {
                Model = model,
                Context = context,
                Message = message,
                Categories = new[] { LoggingData.EnterpiseBaseCategory, modelCategory, loggerCategory, modelLoggerCategory }.ToList(),
                Severity = severity
            };

            _logWriter.Write(logEntry);
        }

        protected void Log(string message, TraceEventType severity, object context)
        {
            var loggerCategory = GetName();
            var logEntry = new ModelLogEntry
            {
                Context = context,
                Message = message,
                Categories = new[] { LoggingData.EnterpiseBaseCategory, loggerCategory }.ToList(),
                Severity = severity
            };

            _logWriter.Write(logEntry);
        }

        protected string GetName()
        {
            var types = new List<Type>();
            var type = GetType();
            types.Add(type);
            types.AddRange(type.GetGenericArguments());
            return string.Join("-", types.Select(t => t.Name));
        }
    }
}
