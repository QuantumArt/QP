using System.IO;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace QP8.Infrastructure.Helpers
{
    public static class XmlSerializerHelpers
    {
        public static string Serialize<T>(T item)
        {
            var xs = new XmlSerializer(item.GetType());
            using (var sw = new StringWriter())
            {
                xs.Serialize(sw, item);
                return sw.ToString();
            }
        }

        public static string Serialize<T>(T item, string rootName)
        {
            var xs = new XmlSerializer(item.GetType(), new XmlRootAttribute(rootName));
            using (var sw = new StringWriter())
            {
                xs.Serialize(sw, item);
                return sw.ToString();
            }
        }

        public static T Deserialize<T>(string xmlString)
        {
            if (string.IsNullOrWhiteSpace(xmlString))
            {
                return default(T);
            }

            using (var ms = new MemoryStream(Encoding.Unicode.GetBytes(xmlString)))
            {
                return (T)new XmlSerializer(typeof(T)).Deserialize(ms);
            }
        }

        public static T Deserialize<T>(XDocument doc)
        {
            var xmlSerializer = new XmlSerializer(typeof(T));
            using (var reader = doc.Root.CreateReader())
            {
                return (T)xmlSerializer.Deserialize(reader);
            }
        }
    }
}
