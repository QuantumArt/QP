using System;
using System.Diagnostics.CodeAnalysis;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Exceptions
{	
	[Serializable]
	[SuppressMessage("Microsoft.Usage", "CA2240")]
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
				var message = (InnerException != null) ? InnerException.Message : string.Empty;
				return string.Format(ContentStrings.VirualSubContentProcessingError, VirtualContent.Id, VirtualContent.Name, message);
			}
		}

		public Content VirtualContent => virtualContent;
	}
}
