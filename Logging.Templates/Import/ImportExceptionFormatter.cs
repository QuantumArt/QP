using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using Quantumart.QP8.BLL.Exceptions;
using Quantumart.QP8.Logging.Loggers;
using Quantumart.QP8.Logging.Transformers;
using System;
using System.IO;

namespace Quantumart.QP8.Logging.Templates.Import
{
	public class ImportExceptionFormatter : TextExceptionFormatter
	{
		public ImportExceptionFormatter(TextWriter writer, Exception exception)
			: base(writer, exception)
		{
		}

		public ImportExceptionFormatter(TextWriter writer, Exception exception, Guid handlingInstanceId)
			: base(writer, exception, handlingInstanceId)
		{
		}

		public override void Format()
		{
		    var importException = Exception as ImportException;
		    if (importException != null)
			{
				var ex = importException;
				var context = new LoggerContext();
				var text = TemplateTransformer.Transform<ImportEndTemplate>(ex.Settings, context);
				Writer.WriteLine(text);
			}

			base.Format();
		}
	}
}
