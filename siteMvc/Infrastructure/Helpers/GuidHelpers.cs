using System;

namespace Quantumart.QP8.WebMvc.Infrastructure.Helpers
{
    public class GuidHelpers
    {
        public static Guid? GetGuidOrDefault(string rawGuid, Guid? defaultValue = null)
        {
            Guid result;
            Guid.TryParse(rawGuid, out result);
            return result == default(Guid) ? defaultValue : result;
        }
    }
}
