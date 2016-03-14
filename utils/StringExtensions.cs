using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quantumart.QP8.Utils
{
	public static class StringExtensions
	{
		public static String ProtectCurlyBrackets(this String s)
		{
			return s.Replace("{", "{{").Replace("}", "}}");
		}

		public static String Left(this String s, int desiredLength)
		{
			if (!String.IsNullOrEmpty(s) && s.Length > desiredLength)
				return s.Substring(0, desiredLength);
			else
				return s;
		}
	}
}
