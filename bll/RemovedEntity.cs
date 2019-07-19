using Newtonsoft.Json;
using Quantumart.QP8.BLL.Converters;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Validators;
using System;

namespace Quantumart.QP8.BLL
{
    public class RemovedEntity
    {
        [LocalizedDisplayName("EntityStringId", NameResourceType = typeof(AuditStrings))]
        public int EntityId { get; set; }

        [LocalizedDisplayName("ParentEntityId", NameResourceType = typeof(AuditStrings))]
        public int ParentEntityId { get; set; }

        [LocalizedDisplayName("EntityTypeName", NameResourceType = typeof(AuditStrings))]
        public string EntityTypeCode { get; set; }

        [LocalizedDisplayName("EntityTitle", NameResourceType = typeof(AuditStrings))]
        public string EntityTitle { get; set; }

        public int UserId { get; set; }

        [LocalizedDisplayName("UserLogin", NameResourceType = typeof(AuditStrings))]
        public string UserLogin { get; set; }

        [JsonConverter(typeof(DateTimeConverter))]
        [LocalizedDisplayName("ExecutionTime", NameResourceType = typeof(AuditStrings))]
        public DateTime DeletedTime { get; set; }
    }
}
