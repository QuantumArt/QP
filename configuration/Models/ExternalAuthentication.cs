using Quantumart.QP8.Configuration.Enums;

namespace Quantumart.QP8.Configuration.Models;

public class ExternalAuthentication
{
    public bool Enabled { get; set; }
    public ExternalAuthenticationType Type { get; set; } = ExternalAuthenticationType.None;
    public bool DisableInternalAccounts { get; set; }

    public bool AllowLoginWithCredentials => !Enabled
       || (Enabled
           && (!DisableInternalAccounts
               || Type == ExternalAuthenticationType.ActiveDirectory));

    public bool ShowSsoButton => Enabled && Type == ExternalAuthenticationType.KeyCloak;

    public bool ShowAutoLoginMessage => Enabled && Type == ExternalAuthenticationType.ActiveDirectory;
}
