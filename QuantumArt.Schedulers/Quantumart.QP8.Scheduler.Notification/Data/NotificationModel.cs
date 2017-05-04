using System.Collections.Generic;
using System.Linq;

namespace Quantumart.QP8.Scheduler.Notification.Data
{
    public class NotificationModel
    {
        public string Url { get; set; }

        public IEnumerable<string> OldXmlNodes { get; set; }

        public IEnumerable<string> NewXmlNodes { get; set; }

        public List<KeyValuePair<string, string>> Parameters
        {
            get
            {
                var list = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("eventName", EventName),
                    new KeyValuePair<string, string>("newXml", CombineNodes(NewXmlNodes, NotificationModelDataType.Articles, NotificationModelFormatType.Xml)),
                    new KeyValuePair<string, string>("oldXml", CombineNodes(OldXmlNodes, NotificationModelDataType.Articles, NotificationModelFormatType.Xml))
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

        public static string CombineNodes(IEnumerable<string> nodes, NotificationModelDataType dataType, NotificationModelFormatType formatType)
        {
            return $"<?xml version=\"1.0\" encoding=\"utf-8\"?><articles>{string.Join(string.Empty, nodes)}</articles>";
        }
    }
}
