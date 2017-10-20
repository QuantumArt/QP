using System;

namespace Quantumart.QP8.BLL.Exceptions
{
	[Serializable]
	public class UserQueryContentCreateViewException : ApplicationException
	{
		public UserQueryContentCreateViewException() { }
        public UserQueryContentCreateViewException(string message) : base(message) { }
		public UserQueryContentCreateViewException(string message, Exception innerException) : base(message, innerException) { }
	}
}
