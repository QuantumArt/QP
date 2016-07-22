using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityFramework6.Test.DataContext
{
    public partial class StatusType
    {
        public Int32 Id { get; set; }
        public string StatusTypeName { get; set; }
        public int Weight { get; set; }
        public Int32 SiteId { get; set; }
    }
}
