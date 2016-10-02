using System.Web;
using Quantumart.QP8.WebMvc.Infrastructure.Constants.XmlDbUpdate;

namespace Quantumart.QP8.WebMvc.Infrastructure.Extensions
{
    public static class HttpContextBaseExtensions
    {
        internal static bool IsXmlDbUpdateReplayAction(this HttpContextBase context)
        {
            return context.Items.Contains(XmlDbUpdateCommonConstants.IsReplayingXmlContext);
        }
    }
}
