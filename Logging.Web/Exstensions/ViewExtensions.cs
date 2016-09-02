using System.Collections.Generic;
using System.Web.Mvc;
using Quantumart.QP8.Logging.Services;
using Quantumart.QP8.Logging.Web.Services;

namespace Quantumart.QP8.Logging.Web.Exstensions
{
    public static class ViewExtensions
    {
        public static MvcHtmlString LogData(this HtmlHelper htmlHelper)
        {
            return new MvcHtmlString(GetService().Read());
        }

        public static MvcHtmlString LogData(this HtmlHelper htmlHelper, IEnumerable<string> listeners)
        {
            return new MvcHtmlString(GetService().Read(listeners));
        }

        private static ILogReader GetService()
        {
            return new LogReader();
        }
    }
}
