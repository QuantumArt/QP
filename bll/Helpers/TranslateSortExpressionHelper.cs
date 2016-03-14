using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quantumart.QP8.BLL.Helpers
{
	public class TranslateHelper
	{
		public static string TranslateSortExpression(string sortExpression, Dictionary<string, string> replaces = null, string @default = "Id")
		{
            string result = sortExpression;
			replaces = replaces ?? new Dictionary<string, string>(0);
            if (!String.IsNullOrEmpty(result))
            {
                string[] parts = result.Split(' ');
                if (parts.Length == 2)
                {
					foreach (var replace in replaces)
					{
						parts[0] = parts[0].Replace(replace.Key, replace.Value);
					}
                    result = String.Join(" ", parts);
                }
            }
            else
				result = @default;
            return result;
		}
	}
}
