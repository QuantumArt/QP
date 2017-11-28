using System.Collections.Generic;

namespace Quantumart.QP8.BLL.Helpers
{
    public class TranslateHelper
    {
        public static string TranslateSortExpression(string sortExpression, Dictionary<string, string> replaces = null, string @default = "Id")
        {
            var result = sortExpression;
            replaces = replaces ?? new Dictionary<string, string>(0);
            if (!string.IsNullOrEmpty(result))
            {
                var parts = result.Split(' ');
                if (parts.Length == 2)
                {
                    foreach (var replace in replaces)
                    {
                        parts[0] = parts[0].Replace(replace.Key, replace.Value);
                    }

                    result = string.Join(" ", parts);
                }
            }
            else
            {
                result = @default;
            }

            return result;
        }
    }
}
