using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Web;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Constants.Mvc;
using Quantumart.QP8.WebMvc.Infrastructure.Extensions;

namespace Quantumart.QP8.WebMvc.Infrastructure.Helpers
{
    internal static class CommonHelpers
    {
        internal static string GetAssemblyVersion()
        {
            return FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location).FileVersion;
        }

        internal static string GetBackendUrl(HttpContextBase context)
        {
            if (context.IsXmlDbUpdateReplayAction())
            {
                return context.Items.Contains(HttpContextItems.BackendUrlContext) ? context.Items[HttpContextItems.BackendUrlContext].ToString() : string.Empty;
            }

            var request = context.Request;
            return $"{request.Url.Scheme}://{request.Url.Host}:{request.Url.Port}{request.ApplicationPath}/";
        }

        internal static HashSet<string> GetDbIdentityInsertOptions(bool disableFieldIdentity, bool disableContentIdentity)
        {
            var identityTypes = new HashSet<string>();
            if (!disableFieldIdentity)
            {
                identityTypes.Add(EntityTypeCode.Field);
                identityTypes.Add(EntityTypeCode.ContentLink);
            }

            if (!disableContentIdentity)
            {
                identityTypes.Add(EntityTypeCode.Content);
                identityTypes.Add(EntityTypeCode.ContentGroup);
            }

            return identityTypes;
        }
    }
}
