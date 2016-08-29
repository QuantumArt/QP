using System.Globalization;

namespace Quantumart.QP8.WebMvc.Infrastructure.Helpers
{
    public static class CultureHelpers
    {
        public static CultureInfo GetCultureInfoByLcid(int lcid)
        {
            return lcid == 0 ? CultureInfo.InvariantCulture : new CultureInfo(lcid);
        }
    }
}
