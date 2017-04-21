using System.Collections.Generic;
using System.Linq;

namespace Quantumart.QP8.Scheduler.Notification.Data
{
    public class NotificationModel
    {
        private const string XmlTemplate = "<?xml version=\"1.0\" encoding=\"utf-8\"?><articles>{0}</articles>";

        public string Url { get; set; }

        public IEnumerable<string> OldXmlNodes { get; set; }

        public IEnumerable<string> NewXmlNodes { get; set; }

        public string OldXml => GetXml(OldXmlNodes);

        public string NewXml => GetXml(NewXmlNodes);

        public List<KeyValuePair<string, string>> Parameters
        {
            get
            {
                var list = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("eventName", EventName),
                    new KeyValuePair<string, string>("newXml", NewXml),
                    new KeyValuePair<string, string>("oldXml", OldXml)
                };

                list.AddRange(Ids.Select(n => new KeyValuePair<string, string>("id", n.ToString())));
                if (ContentId.HasValue)
                {
                    list.Add(new KeyValuePair<string, string>("contentId", ContentId.Value.ToString()));
                }

                if (SiteId.HasValue)
                {
                    list.Add(new KeyValuePair<string, string>("siteId", SiteId.Value.ToString()));
                }

                return list;
            }
        }

        public IEnumerable<int> Ids { get; set; }

        public int? ContentId { get; set; }

        public int? SiteId { get; set; }

        public string EventName { get; set; }

        public static string GetXml(IEnumerable<string> nodes)
        {
            return string.Format(XmlTemplate, string.Join(string.Empty, nodes));
        }
    }
}
