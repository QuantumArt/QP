using Newtonsoft.Json;
using System.Xml.Serialization;

namespace Quantumart.QP8.Configuration.Models
{
    public class QaConfigCustomer
    {
        [JsonProperty("name")]
        [XmlAttribute("customer_name")]
        public string CustomerName { get; set; }

        [JsonProperty("excludeFromSchedulers")]
        [XmlAttribute("exclude_from_schedulers")]
        public bool ExcludeFromSchedulers { get; set; }

        [JsonIgnore]
        [XmlAttribute("exclude_from_schedulers_cdcelastic")]
        public bool ExcludeFromSchedulersCdcElastic { get; set; }

        [JsonIgnore]
        [XmlAttribute("exclude_from_schedulers_cdctarantool")]
        public bool ExcludeFromSchedulersCdcTarantool { get; set; }

        [JsonProperty("connectionString")]
        [XmlElement("db")]
        public string ConnectionString { get; set; }
    }
}
