using System;
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

        internal static Guid? GetGuidForSubstitution(this HttpContextBase context)
        {
            return IsXmlDbUpdateReplayAction(context) && context.Items.Contains(XmlDbUpdateCommonConstants.XmlContextGuidSubstitution)
                ? context.Items[XmlDbUpdateCommonConstants.XmlContextGuidSubstitution] as Guid?
                : null;
        }
    }
}
