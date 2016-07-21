using System;
using System.Globalization;
using System.Text;

namespace qp8dbupdate
{
    public static class Extentions
    {
        public static bool QAIsNullOrEmpty(this string text)
        {
            return string.IsNullOrEmpty(text);
        }

        public static string QAErrorMessage(this Exception ex)
        {
            if (ex == null) return string.Empty;

            string innerMsg = ex.InnerException.QAErrorMessage();
            var sb = new StringBuilder();
            if (innerMsg.Length > 0)
            {
                sb.AppendFormat(CultureInfo.InvariantCulture, "{0}\r\n", innerMsg);
                sb.AppendLine("-");
            }
            sb.AppendFormat(CultureInfo.InvariantCulture, "{0}: {1}\r\n", ex.GetType().Name, ex.Message);
            if (!ex.Source.QAIsNullOrEmpty())
            {
                sb.AppendFormat(CultureInfo.InvariantCulture, "Source: {0}\r\n", ex.Source);
            }
            if (ex.TargetSite != null)
            {
                sb.AppendFormat(CultureInfo.InvariantCulture, "Target Site: {0}\r\n", ex.TargetSite);
            }
            if (!ex.StackTrace.QAIsNullOrEmpty())
            {
                sb.AppendFormat(CultureInfo.InvariantCulture, "Stack Trace:\r\n{0}", ex.StackTrace);
            }
            return sb.ToString();
        }
    }
}
