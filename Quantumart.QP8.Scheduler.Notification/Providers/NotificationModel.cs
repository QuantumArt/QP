using System.Collections.Generic;
using System.Net.Http;

namespace Quantumart.QP8.Scheduler.Notification.Providers
{
    public class NotificationModel
    {
        private const string XmlTemplate = "<?xml version=\"1.0\" encoding=\"utf-8\"?><articles>{0}</articles>";

        public string Url { get; set; }
        public IEnumerable<string> OldXmlNodes { get; set; }
        public IEnumerable<string> NewXmlNodes { get; set; }

        public string OldXml => GetXml(OldXmlNodes);

        public string NewXml => GetXml(NewXmlNodes);

        public FormUrlEncodedContent Content => new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("NEW_XML", NewXml),
            new KeyValuePair<string, string>("OLD_XML", OldXml)
        });

        public static string GetXml(IEnumerable<string> nodes)
        {
            return string.Format(XmlTemplate, string.Join(string.Empty, nodes));
        }
    }
}
