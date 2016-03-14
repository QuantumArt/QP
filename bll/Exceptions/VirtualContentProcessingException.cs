using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Exceptions
{	
	[Serializable]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2240")]
	public class VirtualContentProcessingException : ApplicationException
	{
		private readonly Content virtualContent;

		public VirtualContentProcessingException(Content virtualContent) : base(null)
		{
			this.virtualContent = virtualContent;
		}
		public VirtualContentProcessingException(Content virtualContent, Exception innerException) : base(null, innerException) 
		{
			this.virtualContent = virtualContent;
		}

		public override string Message
		{
			get
			{
				string message = (InnerException != null) ? InnerException.Message : String.Empty;
				return String.Format(ContentStrings.VirualSubContentProcessingError, VirtualContent.Id, VirtualContent.Name, message);
			}
		}

		public Content VirtualContent { get { return virtualContent; } }
	}
}
