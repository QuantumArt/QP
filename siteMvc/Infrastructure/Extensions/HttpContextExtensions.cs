using System;
using Microsoft.AspNetCore.Http;
using Quantumart.QP8.Constants.Mvc;

namespace Quantumart.QP8.WebMvc.Infrastructure.Extensions
{
    public static class HttpContextExtensions
    {
        internal static bool IsXmlDbUpdateReplayAction(this HttpContext context)
        {
            return context.Items.ContainsKey(HttpContextItems.IsReplayingXmlContext);
        }

        internal static Guid? GetGuidForSubstitution(this HttpContext context)
        {
            if (IsXmlDbUpdateReplayAction(context) &&
                context.Items.TryGetValue(HttpContextItems.XmlContextGuidSubstitution, out object value) &&
                value is Guid guid)
            {
                return guid;
            }
            return null;
        }
    }
}
