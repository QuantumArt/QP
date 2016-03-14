using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using Microsoft.Practices.EnterpriseLibrary.Logging.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Logging.Formatters;
using Quantumart.QP8.Logging.Transformers;

namespace Quantumart.QP8.Logging.Formatters
{
	[ConfigurationElementType(typeof(CustomFormatterData))]
	public class TemplateFormatter : LogFormatter
	{
		#region Constants
		private const string DefaultTemplateKey = "Default";
		private const string ErrorTemplateKey = "Error";
		#endregion

		private readonly Dictionary<string, string> _templateMap;
		public TraceOptions TraceOutputOptions { get; set; }

		public TemplateFormatter(NameValueCollection data)
		{
			TraceOutputOptions = TraceOptions.None;
			_templateMap = data.AllKeys.ToDictionary(key => key, key => data[key]);
		}

		public string Format(LogEntry logEntry, TraceOptions traceOptions)
		{
			object model = null;
			object context = null;
			string defaultTemplate = null;
			string errorTemplate = null;
			string template = null;
			string currentTemplate = null;
			var modelEntry = logEntry as ModelLogEntry;

			if (modelEntry != null)
			{
				model = modelEntry.Model;
				context = modelEntry.Context;
			}

			_templateMap.TryGetValue(ErrorTemplateKey, out errorTemplate);
			_templateMap.TryGetValue(DefaultTemplateKey, out defaultTemplate);

			if (model != null)
			{
				_templateMap.TryGetValue(model.GetType().Name, out template);
			}

			currentTemplate = logEntry.Severity == TraceEventType.Error ? errorTemplate : template ?? defaultTemplate;

			return TemplateTransformer.Transform(currentTemplate, logEntry, model, context, traceOptions);
		}

		public override string Format(LogEntry logEntry)
		{
			return Format(logEntry, TraceOutputOptions);
		}
	}
}
