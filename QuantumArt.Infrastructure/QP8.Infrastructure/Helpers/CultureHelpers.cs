using System.Globalization;

namespace QP8.Infrastructure.Helpers
{
    public static class CultureHelpers
    {
        public static CultureInfo GetCultureByLcid(int lcid) => lcid == 0 ? CultureInfo.InvariantCulture : new CultureInfo(lcid);
    }
}
