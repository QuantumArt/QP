using System.Configuration;
using Quantumart.QP8.Configuration.Authentication.WindowsAuthentication;

namespace Quantumart.QP8.Configuration.Authentication
{
    public class AuthenticationElement : ConfigurationElement
    {
        [ConfigurationProperty("allowSaveUserInformationInCookie", DefaultValue = true, IsRequired = true)]
        public bool AllowSaveUserInformationInCookie
        {
            get => (bool)base["allowSaveUserInformationInCookie"];
            set => base["allowSaveUserInformationInCookie"] = value;
        }

        [ConfigurationProperty("windowsAuthentication", IsRequired = true)]
        public WindowsAuthenticationElement WindowsAuthentication => (WindowsAuthenticationElement)base["windowsAuthentication"];
    }
}
