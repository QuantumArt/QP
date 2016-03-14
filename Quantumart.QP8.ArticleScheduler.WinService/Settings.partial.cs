using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quantumart.QP8.ArticleScheduler.WinService.Properties
{
	internal sealed partial class Settings
	{
		public static IEnumerable<string> SplitExceptCustomerCodes(string exceptCustomerCodes)
		{
			if (String.IsNullOrWhiteSpace(exceptCustomerCodes))
				return Enumerable.Empty<string>();

			return exceptCustomerCodes.Split(';')
				.Select(c => c.Trim())
				.Where(c => !String.IsNullOrWhiteSpace(c))
				.ToArray();
		}
	}
}
