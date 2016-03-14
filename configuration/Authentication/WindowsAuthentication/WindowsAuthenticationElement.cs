using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Web;
using System.Web.Configuration;

namespace Quantumart.QP8.Configuration.Authentication.WindowsAuthentication
{
    public class WindowsAuthenticationElement : ConfigurationElement
    {
        [ConfigurationProperty("loginUrl", IsRequired = true)]
        public string LoginUrl
        {
            get { return Utils.Url.ToAbsolute(base["loginUrl"].ToString()); }
            set { base["loginUrl"] = value; }
        }

        [ConfigurationProperty("ipRanges")]
        public IpRangeCollection IpRanges
        {
            get { return ((IpRangeCollection)(base["ipRanges"])); }
        }
    }
}
