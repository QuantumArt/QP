using Quantumart.QP8.Configuration.Enums;

namespace Quantumart.QP8.Configuration.Models;

public class ExternalAuthentication
{
    public bool Enabled { get; set; }
    public ExternalAuthenticationType ExternalAuthenticationType { get; set; } = ExternalAuthenticationType.None;
    public bool DisableInternalAccounts { get; set; }

    public bool AllowLoginWithCredentials => !Enabled
       || (Enabled
           && (!DisableInternalAccounts
               || ExternalAuthenticationType == ExternalAuthenticationType.ActiveDirectory));

    public bool ShowSsoButton => Enabled && ExternalAuthenticationType == ExternalAuthenticationType.KeyCloak;

    public bool ShowAutoLoginMessage => Enabled && ExternalAuthenticationType == ExternalAuthenticationType.ActiveDirectory;
}
