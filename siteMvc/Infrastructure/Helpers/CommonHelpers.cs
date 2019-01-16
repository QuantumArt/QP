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
        internal static string GetAssemblyVersion() => FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location).FileVersion;

        internal static string GetBackendUrl(HttpContextBase context)
        {
            if (context.IsXmlDbUpdateReplayAction())
            {
                return context.Items.Contains(HttpContextItems.BackendUrlContext) ? context.Items[HttpContextItems.BackendUrlContext].ToString() : string.Empty;
            }

            return $"{context.Request.Url?.Scheme}://{context.Request.Url?.Host}:{context.Request.Url?.Port}{context.Request.ApplicationPath}/";
        }

        internal static HashSet<string> GetDbIdentityInsertOptions(bool generateNewFieldIds, bool generateNewContentIds)
        {
            var identityTypes = new HashSet<string>();
            if (!generateNewFieldIds)
            {
                identityTypes.Add(EntityTypeCode.Field);
                identityTypes.Add(EntityTypeCode.ContentLink);
            }

            if (!generateNewContentIds)
            {
                identityTypes.Add(EntityTypeCode.Content);
                identityTypes.Add(EntityTypeCode.ContentGroup);
            }

            return identityTypes;
        }
    }
}
