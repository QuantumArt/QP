using System;

namespace Quantumart.QP8.WebMvc.Infrastructure.Helpers
{
    internal class GuidHelpers
    {
        internal static Guid? GetGuidOrDefault(string rawGuid, Guid? defaultValue = null)
        {
            Guid result;
            Guid.TryParse(rawGuid, out result);
            return result == default(Guid) ? defaultValue : result;
        }
    }
}
