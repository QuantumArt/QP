using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;

namespace Quantumart.QP8.WebMvc.Infrastructure.Extensions
{
    public static class HtmlContentExtensions
    {
        public static void WriteToStringBuilder(this IHtmlContent content, StringBuilder builder)
        {
            using (var writer = new StringWriter(builder))
            {
                content.WriteTo(writer, HtmlEncoder.Default);
            }
        }

        public static string ToHtmlEncodedString(this IHtmlContent content)
        {
            using (var writer = new StringWriter())
            {
                content.WriteTo(writer, HtmlEncoder.Default);
                return writer.ToString();
            }
        }
    }
}
