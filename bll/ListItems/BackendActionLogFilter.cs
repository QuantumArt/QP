using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quantumart.QP8.BLL.ListItems
{
    public class BackendActionLogFilter
    {
        public string actionCode { get; set; }
        public string actionTypeCode { get; set; }
        public string entityTypeCode { get; set; }
        public string parentEntityId { get; set; }
        public string entityStringId { get; set; }
        public string entityTitle { get; set; }
        public string from { get; set; }
        public string to { get; set; }
        public int[] userIDs { get; set; }
    }
}
