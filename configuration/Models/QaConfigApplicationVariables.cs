using System.Xml.Serialization;

namespace Quantumart.QP8.Configuration.Models
{
    public class QaConfigApplicationVariablesElement
    {
        [XmlElement("app_var")]
        public QaConfigApplicationVariable[] Variables { get; set; }
    }
}
