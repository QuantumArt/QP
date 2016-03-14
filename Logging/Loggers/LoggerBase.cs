using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using Microsoft.Practices.EnterpriseLibrary.Logging;

namespace Quantumart.QP8.Logging.Loggers
{
	public class LoggerBase
	{
		#region Constants
		private const string ExceptionPolicy = "Policy";
		private const string BaseCategory = "Custom";
		#endregion

		#region Properties
		private readonly LogWriter _logWriter;
		private readonly ExceptionManager _exceptionManager;
		#endregion

		public LoggerBase(LogWriter logWriter, ExceptionManager exceptionManager)
		{
			_logWriter = logWriter;
			_exceptionManager = exceptionManager;
		}

		protected void LogException(Exception ex)
		{
			_exceptionManager.HandleException(ex, ExceptionPolicy);
		}

		protected void Log(string message, TraceEventType severity, object model, object context)
		{
			string modelCategory = model.GetType().Name;
			string loggerCategory = GetName();
			string modelLoggerCategory = loggerCategory + "/" + modelCategory;

			var logEntry = new ModelLogEntry
			{
				Model = model,
				Context = context,
				Message = message,
				Categories = new[] { BaseCategory, modelCategory, loggerCategory, modelLoggerCategory }.ToList(),
				Severity = severity
			};

			_logWriter.Write(logEntry);
		}

		protected void Log(string message, TraceEventType severity, object context)
		{
			string loggerCategory = GetName();

			var logEntry = new ModelLogEntry
			{
				Context = context,
				Message = message,
				Categories = new[] { BaseCategory, loggerCategory }.ToList(),
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