using System.Collections.Generic;

namespace Quantumart.QP8.BLL.Models.NotificationSender
{
    public class CdcEntityModel
    {
        public string EntityType { get; set; }

        public string InvariantName { get; set; }

        public IDictionary<string, object> Columns { get; set; }

        public IDictionary<string, object> MetaData { get; set; }
    }
}
