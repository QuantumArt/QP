using System;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Quantumart.QP8.WebMvc.Infrastructure.Extensions
{
    public static class XDocumentExtensions
    {
        public static string ToNormalizedString(this XDocument doc, bool withoutXmlDeclaration = false)
        {
            return doc.ToNormalizedString(SaveOptions.None, withoutXmlDeclaration);
        }

        public static string ToNormalizedString(this XDocument doc, SaveOptions saveOptions, bool withoutXmlDeclaration = false)
        {
            if (doc == null)
            {
                throw new ArgumentNullException(nameof(doc));
            }

            var xmlSettings = new XmlWriterSettings
            {
                Encoding = Encoding.UTF8,
                NamespaceHandling = NamespaceHandling.OmitDuplicates
            };

            if (withoutXmlDeclaration)
            {
                xmlSettings.OmitXmlDeclaration = true;
            }

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

        public static string ToNormalizedString(this XElement element)
        {
            return ToNormalizedString(new XDocument(element));
        }

        public static string ToNormalizedString(this XElement element, SaveOptions saveOptions, bool withoutXmlDeclaration = false)
        {
            return ToNormalizedString(new XDocument(element), saveOptions, withoutXmlDeclaration);
        }
    }
}
