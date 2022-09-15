using Microsoft.Extensions.Options;
using System;
using System.Text;

namespace Quantumart.QP8.Security.Ldap;

public class LdapHelper
{
    private readonly IOptions<LdapSettings> _options;

    public LdapHelper(IOptions<LdapSettings> options)
    {
        _options = options;
    }

    public string BuildDomainName(string login)
    {
        return $"{_options.Value.Domain}\\{login}";
    }

    public static DateTime FromAccountExpiresToDateTime(long ticks)
        => new DateTime(1601, 01, 01, 0, 0, 0, DateTimeKind.Utc).AddTicks(ticks);

    public static string ConvertGuidToOctetString(Guid guid)
    {
        var sb = new StringBuilder();

        foreach (var b in guid.ToByteArray())
        {
            _ = sb.Append('\\');
            _ = sb.Append($"{b:X2}");
        }

        return sb.ToString();
    }

    public static Guid ConvertOctetStringToGuid(byte[] octet) => new(octet);

    /// <remarks>
    /// https://docs.microsoft.com/ru-ru/troubleshoot/windows-server/identity/useraccountcontrol-manipulate-account-properties
    /// </remarks>
    public static class UserAccountControlCodes
    {
        public static bool HasFlag(int source, int value) => (source & value) > 0;

        /// <summary>
        /// The user account is disabled.
        /// </summary>
        public const int AccountDisable = 0x0002;

        /// <summary>
        /// It's a default account type that represents a typical user.
        /// </summary>
        public const int NormalAccount = 0x0200;

        /// <summary>
        /// (Windows 2000/Windows Server 2003) The user's password has expired.
        /// </summary>
        public const int PasswordExpired = 0x800000;
    }
}
