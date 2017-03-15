using System;

namespace Quantumart.QP8.EntityFramework.Models
{
    public partial class StatusType
    {
        public Int32 Id { get; set; }
        public string StatusTypeName { get; set; }
        public int Weight { get; set; }
        public Int32 SiteId { get; set; }
    }
}
