using System.Globalization;

namespace Quantumart.QP8.WebMvc.Infrastructure.Helpers
{
    internal static class CultureHelpers
    {
        internal static CultureInfo GetCultureInfoByLcid(int lcid) => lcid == 0 ? CultureInfo.InvariantCulture : new CultureInfo(lcid);
    }
}
