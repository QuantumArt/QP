using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Web;
using System.Web.Configuration;

namespace Quantumart.QP8.Configuration.Authentication.WindowsAuthentication
{
    public class IpRangeElement : System.Configuration.ConfigurationElement
    {
        [System.Configuration.ConfigurationProperty("name", DefaultValue = "", IsKey = true, IsRequired = true)]
        public string Name
        {
            get { return ((string)(base["name"])); }
            set { base["name"] = value; }
        }

        [System.Configuration.ConfigurationProperty("beginIp", DefaultValue = "", IsKey = false, IsRequired = false)]
        public string BeginIp
        {
            get { return ((string)(base["beginIp"])); }
            set { base["beginIp"] = value; }
        }

        [System.Configuration.ConfigurationProperty("endIp", DefaultValue = "", IsKey = false, IsRequired = false)]
        public string EndIp
        {
            get { return ((string)(base["endIp"])); }
            set { base["endIp"] = value; }
        }
    }
}
