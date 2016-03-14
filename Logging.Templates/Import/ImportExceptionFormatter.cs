using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using Quantumart.QP8.BLL.Exceptions;
using Quantumart.QP8.Logging.Loggers;
using Quantumart.QP8.Logging.Transformers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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
			if (Exception is ImportException)
			{
				var ex = (ImportException)Exception;
				var context = new LoggerContext();
				string text = TemplateTransformer.Transform<ImportEndTemplate>(ex.Settings, context);
				Writer.WriteLine(text);
			}

			base.Format();
		}	
	}
}
