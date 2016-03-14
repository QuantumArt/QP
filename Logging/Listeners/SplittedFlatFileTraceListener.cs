using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Practices.EnterpriseLibrary.Logging.Formatters;
using Microsoft.Practices.EnterpriseLibrary.Logging.TraceListeners;
using Quantumart.QP8.Logging.Formatters;

namespace Quantumart.QP8.Logging.Listeners
{
	public abstract class SplittedFlatFileTraceListener : CustomTraceListener
	{
		#region Properties
		protected Dictionary<string, FlatFileTraceListener> Listeners { get; private set; }
		protected string FileName { get; private set; }
		protected string Header { get; private set; }
		protected string Footer { get; private set; }
		#endregion

		#region Constructor
		public SplittedFlatFileTraceListener(string fileName, string header, string footer, ILogFormatter formatter, TraceOptions traceOutputOptions)
			: base()
		{
			Listeners = new Dictionary<string, FlatFileTraceListener>();
			FileName = fileName;
			Header = header;
			Footer = footer;
			Formatter = formatter;
			TraceOutputOptions = traceOutputOptions;
		}
		#endregion

		#region Overrides
		public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, object data)
		{
			var listener = GetTraceListener(data);
			listener.TraceData(eventCache, source, eventType, id, data);
		}

		public override void Flush()
		{
			foreach (var listener in Listeners.Values)
			{
				listener.Flush();
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				foreach (var listener in Listeners.Values)
				{
					listener.Dispose();
				}

				Listeners.Clear();
			}

			base.Dispose(disposing);
		}

		public override void Write(string message)
		{
		}

		public override void WriteLine(string message)
		{
		}
		#endregion

		#region Protected methods
		protected FlatFileTraceListener GetTraceListener(object data)
		{
			string currentFileName = GetCurrentFileName(data);
			FlatFileTraceListener listener;

			if (!Listeners.TryGetValue(currentFileName, out listener))
			{
				listener = new FlatFileTraceListener(currentFileName, Header, Footer, Formatter);
				listener.TraceOutputOptions = TraceOutputOptions;
				var templateFormatter =  listener.Formatter as TemplateFormatter;
				if (templateFormatter != null)
				{
					templateFormatter.TraceOutputOptions = TraceOutputOptions;
				}
				Listeners[currentFileName] = listener;
			};

			return listener;		
		}

		protected abstract string GetCurrentFileName(object data);
		#endregion
	}
}