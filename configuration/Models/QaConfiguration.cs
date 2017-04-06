using System.Xml.Serialization;

namespace Quantumart.QP8.Configuration.Models
{
    [XmlRoot("configuration")]
    public class QaConfiguration
    {
        [XmlElement("app_vars")]
        public QaConfigApplicationVariablesElement RootApplicationVariablesElement { get; set; }

        [XmlElement("customers")]
        public QaConfigurationCustomersElement RootCustomersElement { get; set; }

        [XmlIgnore]
        public QaConfigApplicationVariable[] Variables => RootApplicationVariablesElement.Variables;

        [XmlIgnore]
        public QaConfigCustomer[] Customers => RootCustomersElement.Customers;
    }
}
