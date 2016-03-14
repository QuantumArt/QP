using System;
using System.Diagnostics;
using System.Linq.Expressions;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration.ContainerModel;
using Microsoft.Practices.EnterpriseLibrary.Logging.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Logging.Formatters;

namespace Quantumart.QP8.Logging.Loggers.Listeners
{
	public class FlatFileSplittedByCustomerCodeTraceListenerData : FlatFileTraceListenerData
	{
		#region Constructors
		public FlatFileSplittedByCustomerCodeTraceListenerData()
			: base()
		{
		}

		public FlatFileSplittedByCustomerCodeTraceListenerData(string fileName, string formatterName)
			: base(fileName, formatterName)
		{
		}

		public FlatFileSplittedByCustomerCodeTraceListenerData(string name, string fileName, string formatterName)
			: base(name, fileName, formatterName)
		{
		}

		public FlatFileSplittedByCustomerCodeTraceListenerData(string name, Type listenerType, string fileName, string formatterName)
			: base(name, listenerType, fileName, formatterName)
		{
		}

		public FlatFileSplittedByCustomerCodeTraceListenerData(string name, string fileName, string header, string footer, string formatterName)
			: base(name, fileName, header, footer, formatterName)
		{
		}

		public FlatFileSplittedByCustomerCodeTraceListenerData(string name, Type listenerType, string fileName, string formatterName, TraceOptions traceOutputOptions)
			: base(name, listenerType, fileName, formatterName, traceOutputOptions)
		{
		}

		public FlatFileSplittedByCustomerCodeTraceListenerData(string name, string fileName, string header, string footer, string formatterName, TraceOptions traceOutputOptions)
			: base(name, fileName, header, footer, formatterName, traceOutputOptions)
		{
		}
		#endregion

		#region Overrides
		protected override Expression<Func<TraceListener>> GetCreationExpression()
		{
			return () => new FlatFileSplittedByCustomerCodeTraceListener(FileName, Header, Footer, Container.ResolvedIfNotNull<ILogFormatter>(Formatter), TraceOutputOptions);
		}
		#endregion
	}
}
