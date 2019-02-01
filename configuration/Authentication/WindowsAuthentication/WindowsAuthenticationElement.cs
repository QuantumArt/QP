using System.Configuration;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.Configuration.Authentication.WindowsAuthentication
{
    public class WindowsAuthenticationElement : ConfigurationElement
    {
        [ConfigurationProperty("loginUrl", IsRequired = true)]
        public string LoginUrl
        {
            #if !NET_STANDARD
            get => Url.ToAbsolute(base["loginUrl"].ToString());
            #else
            get => base["loginUrl"].ToString();
            #endif

            set => base["loginUrl"] = value;
        }

        [ConfigurationProperty("ipRanges")]
        public IpRangeCollection IpRanges => (IpRangeCollection)base["ipRanges"];
    }
}
