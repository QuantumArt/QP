using System.Text.RegularExpressions;

namespace Quantumart.QP8.Utils
{
    public static class Url
    {

        public static bool IsQueryEmpty(string url)
        {
            return url.IndexOf("?") < 0;
        }
    }
}
