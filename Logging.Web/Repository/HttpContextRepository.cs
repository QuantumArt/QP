using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Web;

namespace Quantumart.QP8.Logging.Web.Repository
{
    internal static class HttpContextRepository
    {
        private const string LogKey = "Log";

        public static StringBuilder GetCurrentOld()
        {
            var context = HttpContext.Current;
            if (context == null)
            {
                return null;
            }

            var sb = context.Items[LogKey] as StringBuilder;
            if (sb == null)
            {
                sb = new StringBuilder();
                context.Items[LogKey] = sb;
            }

            return sb;
        }

        public static ReadOnlyCollection<LogItem> GetCurrent()
        {
            var list = GetCurrentInternal();
            return list?.AsReadOnly() ?? new List<LogItem>().AsReadOnly();
        }

        public static void Add(string text, string listener)
        {
            var list = GetCurrentInternal();

            if (list != null)
            {
                var item = new LogItem(text, listener);
                list.Add(item);
            }
        }

        private static List<LogItem> GetCurrentInternal()
        {
            var context = HttpContext.Current;
            if (context == null)
            {
                return null;
            }

            var list = context.Items[LogKey] as List<LogItem>;
            if (list == null)
            {
                list = new List<LogItem>();
                context.Items[LogKey] = list;
            }

            return list;
        }
    }
}
