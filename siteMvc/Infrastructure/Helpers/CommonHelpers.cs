using System.Diagnostics.CodeAnalysis;
using System.Web;
using Quantumart.QP8.WebMvc.Infrastructure.Constants.XmlDbUpdate;

namespace Quantumart.QP8.WebMvc.Infrastructure.Helpers
{
    internal class CommonHelpers
    {
        internal static bool IsXmlDbUpdateReplayAction(HttpContextBase context)
        {
            return context.Items.Contains(XmlDbUpdateCommonConstants.IsReplayingXmlContext);
        }

        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        internal static string GetBackendUrl(HttpContextBase context)
        {
            if (IsXmlDbUpdateReplayAction(context))
            {
                return context.Items.Contains(XmlDbUpdateCommonConstants.BackendUrlContext) ? context.Items[XmlDbUpdateCommonConstants.BackendUrlContext].ToString() : string.Empty;
            }

            var request = context.Request;
            return $"{request.Url.Scheme}://{request.Url.Host}:{request.Url.Port}{request.ApplicationPath}/";
        }
    }
}
