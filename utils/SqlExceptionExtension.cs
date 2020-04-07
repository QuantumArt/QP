using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;

namespace Quantumart.QP8.Utils
{
	public static class DbExceptionExtension
	{
		public static string ErrorsToString(this SqlException ex)
		{
			StringBuilder errorMessagesBuilder = new StringBuilder();
			for (int i = 0; i < ex.Errors.Count; i++)
			{
				errorMessagesBuilder.AppendFormat("Index #{0}. Message: {1} LineNumber: {2}.", i, ex.Errors[i].Message, ex.Errors[i].LineNumber.ToString());
			}
			return errorMessagesBuilder.ToString();

		}
	}
}
