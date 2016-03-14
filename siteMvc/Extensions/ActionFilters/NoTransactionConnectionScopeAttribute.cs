using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Quantumart.QP8.WebMvc.Extensions.ActionFilters
{
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
	public class NoTransactionConnectionScopeAttribute : ConnectionScopeAttribute
	{
		protected override ConnectionScopeMode _Mode
		{
			get
			{
				return ConnectionScopeMode.TransactionOff;
			}
		}
	}
}