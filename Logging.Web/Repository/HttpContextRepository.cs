using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Web;
using Quantumart.QP8.Constants.Mvc;

namespace Quantumart.QP8.Logging.Web.Repository
{
    internal static class HttpContextRepository
    {
        public static StringBuilder GetCurrentOld()
        {
            if (HttpContext.Current == null)
            {
                return null;
            }

            var sb = HttpContext.Current.Items[HttpContextItems.LogKey] as StringBuilder;
            if (sb == null)
            {
                sb = new StringBuilder();
                HttpContext.Current.Items[HttpContextItems.LogKey] = sb;
            }

            return sb;
        }

        public static ReadOnlyCollection<LogItem> GetCurrent()
        {
            return GetCurrentInternal()?.AsReadOnly() ?? new List<LogItem>().AsReadOnly();
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

            var list = context.Items[HttpContextItems.LogKey] as List<LogItem>;
            if (list == null)
            {
                list = new List<LogItem>();
                context.Items[HttpContextItems.LogKey] = list;
            }

            return list;
        }
    }
}
