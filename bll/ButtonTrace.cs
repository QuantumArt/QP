using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Quantumart.QP8.BLL.Converters;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL
{
    public class ButtonTrace
    {
        [Display(Name = "ButtonName", ResourceType = typeof(AuditStrings))]
        public string ButtonName { get; set; }

        [Display(Name = "TabName", ResourceType = typeof(AuditStrings))]
        public string TabName { get; set; }

        [JsonConverter(typeof(DateTimeConverter))]
        [Display(Name = "ExecutionTime", ResourceType = typeof(AuditStrings))]
        public DateTime ActivatedTime { get; set; }

        public int UserId { get; set; }

        [Display(Name = "UserLogin", ResourceType = typeof(AuditStrings))]
        public string UserLogin { get; set; }
    }
}
