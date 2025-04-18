using System;

namespace Quantumart.QP8.Configuration;

public class KeyCloakSettings
{
    public const string ConfigurationSectionName = "KeyCloak";
    public const string HttpClientName = "KeyCloak";

    public string ApiUrl { get; set; }
    public string Realm { get; set; }
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string AuthClientId { get; set; }
    public string RedirectUrl { get; set; }
    public string EnableSettingName { get; set; } = "KEYCLOAK_ENABLED";

    public string RedirectAddress => new Uri(new(RedirectUrl), "LogOn/SsoCallback").AbsoluteUri;
}
