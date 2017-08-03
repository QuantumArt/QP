using System;
using System.Xml.Serialization;
using QP8.Infrastructure.Extensions;

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
        public QaConfigApplicationVariable[] Variables => RootApplicationVariablesElement?.Variables
            ?? throw new Exception($"<app_vars>{{...appVars}}</app_vars> should be existing at config: {RootApplicationVariablesElement.ToJsonLog()}");

        [XmlIgnore]
        public QaConfigCustomer[] Customers => RootCustomersElement?.Customers
            ?? throw new Exception($"<customers>{{...customerData}}</customers> should be existing at config: {RootCustomersElement.ToJsonLog()}");
    }
}
