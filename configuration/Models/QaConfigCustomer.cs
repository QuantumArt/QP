using System;
using System.Xml.Serialization;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.Configuration.Models
{
    public class QaConfigCustomer
    {
        [XmlAttribute("customer_name")]
        public string CustomerName { get; set; }

        [XmlAttribute("exclude_from_schedulers")]
        public bool ExcludeFromSchedulers { get; set; }

        [XmlIgnore]
        public DatabaseType DbType { get; set; }

        [XmlAttribute("db_type")]
        public string TypeXml
        {
            get { return DbType.ToString().ToLowerInvariant() ; }
            set
            {
                if (Enum.TryParse<DatabaseType>(value, true, out var parsed))
                {
                    DbType = parsed;
                }
            }
        }

        [XmlAttribute("exclude_from_schedulers_cdcelastic")]
        public bool ExcludeFromSchedulersCdcElastic { get; set; }

        [XmlAttribute("exclude_from_schedulers_cdctarantool")]
        public bool ExcludeFromSchedulersCdcTarantool { get; set; }

        [XmlElement("db")]
        public string ConnectionString { get; set; }
    }
}
