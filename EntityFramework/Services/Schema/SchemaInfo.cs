using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quantumart.QP8.EntityFramework.Services
{
    public class SchemaInfo
    {
        public string ConnectionStringName { get; set; }
        public string ClassName { get; set; }
        public string NamespaceName { get; set; }
        public bool UseLongUrls { get; set; }
        public bool ReplaceUrls { get; set; }
        public bool SendNotifications { get; set; }
        public bool DbIndependent { get; set; }
        public string SiteName { get; set; }
        public bool IsPartial { get; set; }
        public string SchemaNamespace { get; set; }
        public string SchemaContainer { get; set; }
        public bool IsStageMode { get; set; }
        public bool LazyLoadingEnabled { get; set; }
    }
}
