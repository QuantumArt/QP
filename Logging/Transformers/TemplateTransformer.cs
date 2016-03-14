using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Practices.EnterpriseLibrary.Logging;

namespace Quantumart.QP8.Logging.Transformers
{
	public static class TemplateTransformer
	{
		#region Public methods
		public static string Transform<T>(LogEntry logEntry, object model, object context, TraceOptions traceOptions)
			where T : class
		{
			var template = Activator.CreateInstance<T>();
			return Transform(template, logEntry, model, context, traceOptions);
		}

		public static string Transform<T>(object model, object context)
			where T : class
		{
			return Transform<T>(null, model, context, TraceOptions.None);
		}

		public static string Transform<T>(LogEntry logEntry, TraceOptions traceOptions)
			where T : class
		{
			if (logEntry == null)
			{
				return string.Empty;
			}
			else
			{
				var template = Activator.CreateInstance<T>();
				return Transform(template, logEntry, null, null, traceOptions);
			}
		}

		public static string Transform(string templateName, LogEntry logEntry, object model, object context, TraceOptions traceOptions)
		{
			Type type = templateName == null ? null : Type.GetType(templateName);

			if (type == null)
			{
				return null;
			}
			else
			{
				var template = Activator.CreateInstance(type);
				return Transform(template, logEntry, model, context, traceOptions);
			}
		}
		#endregion

		#region Exstension methods
		public static void Transform<T>(this LogEntry logEntry, Action<string> write, object model)
			where T : class
		{
			if (logEntry != null)
			{
				write(Transform<T>(logEntry, model));
			}
		}
		#endregion

		#region Private members
		private static string Transform(dynamic template, LogEntry logEntry, object model, object context, TraceOptions traceOptions)
		{
			template.Session = new Dictionary<string, object>();

			if (logEntry != null)
			{
				template.Session.Add("LogEntry", logEntry);
			}

			template.Session.Add("Model", model);
			template.Session.Add("Context", context);
			template.Session.Add("TraceOptions", traceOptions);
			template.Initialize();
			return template.TransformText();
		}
		#endregion
	}
}