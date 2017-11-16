using System.Configuration;

namespace Quantumart.QP8.Configuration.Authentication.WindowsAuthentication
{
    public class IpRangeElement : ConfigurationElement
    {
        [ConfigurationProperty("name", DefaultValue = "", IsKey = true, IsRequired = true)]
        public string Name
        {
            get => (string)base["name"];
            set => base["name"] = value;
        }

        [ConfigurationProperty("beginIp", DefaultValue = "", IsKey = false, IsRequired = false)]
        public string BeginIp
        {
            get => (string)base["beginIp"];
            set => base["beginIp"] = value;
        }

        [ConfigurationProperty("endIp", DefaultValue = "", IsKey = false, IsRequired = false)]
        public string EndIp
        {
            get => (string)base["endIp"];
            set => base["endIp"] = value;
        }
    }
}
