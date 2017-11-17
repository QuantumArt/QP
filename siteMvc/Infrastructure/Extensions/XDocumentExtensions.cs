using System;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using QP8.Infrastructure;

namespace Quantumart.QP8.WebMvc.Infrastructure.Extensions
{
    public static class XDocumentExtensions
    {
        public static string ToNormalizedString(this XDocument doc, bool withoutXmlDeclaration = false) => doc.ToNormalizedString(SaveOptions.None, withoutXmlDeclaration);

        public static string ToNormalizedString(this XDocument doc, SaveOptions saveOptions, bool omitXmlDeclaration = false)
        {
            if (doc == null)
            {
                throw new ArgumentNullException(nameof(doc));
            }

            var xmlSettings = new XmlWriterSettings
            {
                Encoding = Encoding.UTF8,
                OmitXmlDeclaration = omitXmlDeclaration,
                NamespaceHandling = NamespaceHandling.OmitDuplicates
            };

            if (saveOptions == SaveOptions.DisableFormatting)
            {
                xmlSettings.Indent = false;
                xmlSettings.NewLineOnAttributes = false;
                xmlSettings.NewLineHandling = NewLineHandling.None;
            }

            var builder = new StringBuilder();
            using (var sw = new Utf8StringWriter(builder))
            using (var xw = XmlWriter.Create(sw, xmlSettings))
            {
                doc.Save(xw);
            }

            return builder.ToString();
        }

        public static string ToNormalizedString(this XElement element) => ToNormalizedString(new XDocument(element));

        public static string ToNormalizedString(this XElement element, SaveOptions saveOptions, bool withoutXmlDeclaration = false) =>
            ToNormalizedString(new XDocument(element), saveOptions, withoutXmlDeclaration);
    }
}
