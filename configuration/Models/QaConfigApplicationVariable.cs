using Newtonsoft.Json;
using System.Xml.Serialization;

namespace Quantumart.QP8.Configuration.Models
{
    public class QaConfigApplicationVariable
    {
        [JsonProperty("name")]
        [XmlAttribute("app_var_name")]
        public string Name { get; set; }

        [JsonProperty("value")]
        [XmlText]
        public string Value { get; set; }
    }
}
