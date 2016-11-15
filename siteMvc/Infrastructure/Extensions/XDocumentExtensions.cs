using System;
using System.Text;
using System.Xml.Linq;

namespace Quantumart.QP8.WebMvc.Infrastructure.Extensions
{
    public static class XDocumentExtensions
    {
        public static string ToStringWithDeclaration(this XDocument doc)
        {
            return doc.ToStringWithDeclaration(SaveOptions.None);
        }

        public static string ToStringWithDeclaration(this XDocument doc, SaveOptions saveOptions)
        {
            if (doc == null)
            {
                throw new ArgumentNullException(nameof(doc));
            }

            var builder = new StringBuilder();
            using (var writer = new Utf8StringWriter(builder))
            {
                doc.Save(writer, saveOptions);
            }

            return builder.ToString();
        }
    }
}
