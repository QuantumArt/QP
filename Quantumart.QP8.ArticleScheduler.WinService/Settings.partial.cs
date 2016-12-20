using System.Collections.Generic;
using System.Linq;

namespace Quantumart.QP8.ArticleScheduler.WinService.Properties
{
    internal sealed partial class Settings
    {
        public static IEnumerable<string> SplitExceptCustomerCodes(string exceptCustomerCodes)
        {
            if (string.IsNullOrWhiteSpace(exceptCustomerCodes))
            {
                return Enumerable.Empty<string>();
            }

            return exceptCustomerCodes.Split(';')
                .Select(c => c.Trim())
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .ToArray();
        }
    }
}
