using System;
using System.Text;
using System.Xml.Linq;

namespace qp8dbupdate.Infrastructure.Extensions
{
    internal static class StringExtensions
    {
        public static string ToStringWithDeclaration(this XDocument doc)
        {
            if (doc == null)
            {
                throw new ArgumentNullException(nameof(doc));
            }

            var builder = new StringBuilder();
            using (var writer = new Utf8StringWriter(builder))
            {
                doc.Save(writer);
            }

            return builder.ToString();
        }
    }
}
