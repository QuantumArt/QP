using System.Collections.Generic;

namespace Quantumart.QP8.Scheduler.Notification.Data
{
    abstract class NotificationBaseModel
    {
        public string Url { get; set; }

        public IEnumerable<string> OldData { get; set; }

        public IEnumerable<string> NewData { get; set; }

        public List<KeyValuePair<string, string>> Parameters
        {
            get
            {
                return null;
            }
        }

        public abstract string CombineNodes(IEnumerable<string> nodes);
    }
}
