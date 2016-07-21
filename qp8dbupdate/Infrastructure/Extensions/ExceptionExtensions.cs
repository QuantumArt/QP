using System;
using System.Globalization;
using System.Text;

namespace qp8dbupdate.Infrastructure.Extensions
{
    public static class Extentions
    {
        public static string QaErrorMessage(this Exception ex)
        {
            if (ex == null)
            {
                return string.Empty;
            }

            var innerMsg = ex.InnerException.QaErrorMessage();
            var sb = new StringBuilder();
            if (innerMsg.Length > 0)
            {
                sb.AppendFormat(CultureInfo.InvariantCulture, "{0}\r\n", innerMsg);
                sb.AppendLine("-");
            }

            sb.AppendFormat(CultureInfo.InvariantCulture, "{0}: {1}\r\n", ex.GetType().Name, ex.Message);
            if (!string.IsNullOrWhiteSpace(ex.Source))
            {
                sb.AppendFormat(CultureInfo.InvariantCulture, "Source: {0}\r\n", ex.Source);
            }

            if (ex.TargetSite != null)
            {
                sb.AppendFormat(CultureInfo.InvariantCulture, "Target Site: {0}\r\n", ex.TargetSite);
            }

            if (!string.IsNullOrWhiteSpace(ex.StackTrace))
            {
                sb.AppendFormat(CultureInfo.InvariantCulture, "Stack Trace:\r\n{0}", ex.StackTrace);
            }

            return sb.ToString();
        }
    }
}
