using System.Xml.Serialization;

namespace Quantumart.QP8.Configuration.Models
{
    public class QaConfigApplicationVariable
    {
        [XmlAttribute("app_var_name")]
        public string Name { get; set; }

        [XmlText]
        public string Value { get; set; }
    }
}
