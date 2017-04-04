using System.Xml.Serialization;

namespace Quantumart.QP8.Configuration.Models
{
    public class QaConfigurationCustomersElement
    {
        [XmlElement("customer")]
        public QaConfigCustomer[] Customers { get; set; }
    }
}
