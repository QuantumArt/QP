using Newtonsoft.Json;
using Quantumart.QP8.BLL.Converters;
using Quantumart.QP8.Resources;
using System;
using System.ComponentModel.DataAnnotations;

namespace Quantumart.QP8.BLL
{
    public class RemovedEntity
    {
        [Display(Name = "EntityStringId", ResourceType = typeof(AuditStrings))]
        public int EntityId { get; set; }

        [Display(Name = "ParentEntityId", ResourceType = typeof(AuditStrings))]
        public int ParentEntityId { get; set; }

        [Display(Name = "EntityTypeName", ResourceType = typeof(AuditStrings))]
        public string EntityTypeCode { get; set; }

        [Display(Name = "EntityTitle", ResourceType = typeof(AuditStrings))]
        public string EntityTitle { get; set; }

        public int UserId { get; set; }

        [Display(Name = "UserLogin", ResourceType = typeof(AuditStrings))]
        public string UserLogin { get; set; }

        [JsonConverter(typeof(DateTimeConverter))]
        [Display(Name = "ExecutionTime", ResourceType = typeof(AuditStrings))]
        public DateTime DeletedTime { get; set; }
    }
}
