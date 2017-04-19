using System.Text.RegularExpressions;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.Validators.Helpers
{
    public static class UrlValidatorHelpers321
    {
        public static bool IsWebFolderUrl321(string url)
        {
            return IsRelativeWebFolderUrl321(url) || IsAbsoluteWebFolderUrl321(url);
        }

        public static bool IsRelativeWebFolderUrl321(string url)
        {
            return Regex.IsMatch(url, RegularExpressions.RelativeWebFolderUrl);
        }

        public static bool IsAbsoluteWebFolderUrl321(string url)
        {
            return Regex.IsMatch(url, RegularExpressions.AbsoluteWebFolderUrl);
        }
    }
}
