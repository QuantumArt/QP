using System.Globalization;

namespace QP8.Infrastructure.Helpers
{
    public class CultureHelpers
    {
        public CultureInfo GetCultureByLcid(int lcid) => lcid == 0 ? CultureInfo.InvariantCulture : new CultureInfo(lcid);
    }
}
