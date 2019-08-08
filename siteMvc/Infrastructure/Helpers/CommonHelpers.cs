using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Constants.Mvc;
using Quantumart.QP8.WebMvc.Infrastructure.Extensions;

namespace Quantumart.QP8.WebMvc.Infrastructure.Helpers
{
    internal static class CommonHelpers
    {
        internal static string GetAssemblyVersion() => FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location).FileVersion;

        internal static string GetBackendUrl(HttpContext context)
        {
            if (context.IsXmlDbUpdateReplayAction())
            {
                return context.Items.ContainsKey(HttpContextItems.BackendUrlContext) ? context.Items[HttpContextItems.BackendUrlContext].ToString() : string.Empty;
            }

            // TODO: review GetBackendUrl
            return $"{context.Request.Scheme}://{context.Request.Host.Host}:{context.Request.Host.Port}{context.Request.PathBase}/";
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
