using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.WebMvc.Extensions.Helpers;

namespace Quantumart.QP8.WebMvc.Extensions.Helpers.API
{
	[Serializable]
	public class ReplayXmlException : ApplicationException
	{
		private RecordedAction action = null; 

		public ReplayXmlException() { }

		public ReplayXmlException(RecordedAction action) : base(null) 
		{
			this.action = action;
		}

		public ReplayXmlException(RecordedAction action, Exception innerException) : base(null, innerException) 
		{
			this.action = action;		
		}

		public override string Message
		{
			get
			{
				string message = (InnerException != null) ? InnerException.Message : String.Empty;
				if (action != null)
					return String.Format("An error occured while replaying action (Code = {0}, ParentId = {1}, Ids = {2}): {3} ", action.Code, action.ParentId, String.Join(",", action.Ids), message);
				else
					return message;
			}
		}
	}
}
