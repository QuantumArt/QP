using System;
using System.Web;
using Quantumart.QP8.Constants.Mvc;

namespace Quantumart.QP8.WebMvc.Infrastructure.Extensions
{
    public static class HttpContextBaseExtensions
    {
        internal static bool IsXmlDbUpdateReplayAction(this HttpContextBase context) => context.Items.Contains(HttpContextItems.IsReplayingXmlContext);

        internal static Guid? GetGuidForSubstitution(this HttpContextBase context) => IsXmlDbUpdateReplayAction(context) && context.Items.Contains(HttpContextItems.XmlContextGuidSubstitution)
            ? context.Items[HttpContextItems.XmlContextGuidSubstitution] as Guid?
            : null;
    }
}
