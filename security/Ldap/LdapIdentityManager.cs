using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Novell.Directory.Ldap;
using System;
using System.Linq;

namespace Quantumart.QP8.Security.Ldap;

public class LdapIdentityManager : ILdapIdentityManager
{
    /// <summary>
    /// Indicates that the account never expires
    /// </summary>
    /// <remarks>
    /// See https://ldapwiki.com/wiki/AccountExpires
    /// </remarks>
    private static readonly long[] AccountNeverExpiresTicks = { 0, 9223372036854775807 };
    private readonly LdapHelper _ldapHelper;
    private readonly IOptions<LdapSettings> _ldapSetting;
    private readonly LdapConnectionFactory _ldapConnectionFactory;    

    public LdapIdentityManager(LdapConnectionFactory ldapConnectionFactory, LdapHelper ldapHelper,
        IOptions<LdapSettings> ldapSetting,
        ILogger<LdapIdentityManager> logger)
    {
        _ldapConnectionFactory = ldapConnectionFactory;
        _ldapHelper = ldapHelper;
        _ldapSetting = ldapSetting;        
    }

    public string CurrentDomain
    {
        get { return _ldapSetting.Value.Domain; }
    }
    
    public SignInResult PasswordSignIn(string login, string password)
    {
        if (string.IsNullOrEmpty(login))
        {
            throw new ArgumentNullException(nameof(login));
        }

        if (string.IsNullOrEmpty(password))
        {
            throw new ArgumentNullException(nameof(password));
        }

        LdapEntry entry = _ldapConnectionFactory.WithConnection(connection => VerifyCredentialsAndGetUserEntry(connection));

        if (entry is not null)
        {            
            return new SignInResult(SignInStatus.Succeeded);            
        }

        return DetermineLoginProblem(login);

        LdapEntry VerifyCredentialsAndGetUserEntry(ILdapConnection connection)
        {
            try
            {
                connection.Bind(_ldapHelper.BuildDomainName(login), password);
                if (!connection.Bound)
                {
                    return null;
                }
            }
            catch (LdapException)
            {
                return null;
            }

            return GetLdapEntryByLogin(connection, login);
        }
    }
       
    private SignInResult DetermineLoginProblem(string login)
    {
        var (isEntryFound, entry) = TryGetLdapEntry(login);

        if (!isEntryFound)
        {
            return new SignInResult(SignInStatus.NotFound);
        }

        var attributes = entry!.GetAttributeSet();

        if (IsAccountExpired(attributes))
        {
            return new SignInResult(SignInStatus.AccountExpired);
        }

        if (IsAccountLocked(attributes))
        {
            return new SignInResult(SignInStatus.IsLockedOut);
        }

        if (IsPasswordExpired() || IsPwdChangeRequired(attributes))
        {
            return new SignInResult(SignInStatus.PasswordExpired);
        }

        return new SignInResult(SignInStatus.OperationError);

        bool IsAccountExpired(LdapAttributeSet attributes)
        {
            if (attributes.TryGetValue("accountExpires", out var accountExpiresAttr))
            {
                var ticks = long.Parse(accountExpiresAttr.StringValue);
                if (!AccountNeverExpiresTicks.Contains(ticks))
                {
                    var accountExpiresAt = LdapHelper.FromAccountExpiresToDateTime(ticks);
                    if (accountExpiresAt <= DateTime.UtcNow)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        static bool IsPwdChangeRequired(LdapAttributeSet attributes)
        {
            return attributes.TryGetValue("pwdLastSet", out var attr)
                && attr.StringValue == "0";
        }

        static bool IsAccountLocked(LdapAttributeSet attributes)
        {
            if (!attributes.TryGetValue("userAccountControl", out var attr))
            {
                return false;
            }

            var code = int.Parse(attr.StringValue);
            return LdapHelper.UserAccountControlCodes.HasFlag(code,
                LdapHelper.UserAccountControlCodes.AccountDisable);
        }

        bool IsPasswordExpired()
        {
            if (!attributes.TryGetValue("userAccountControl", out var attr))
            {
                return false;
            }

            var code = int.Parse(attr.StringValue);
            return LdapHelper.UserAccountControlCodes.HasFlag(code,
                LdapHelper.UserAccountControlCodes.PasswordExpired);
        }
    }
        

    private LdapEntry GetLdapEntryByLogin(ILdapConnection connection, string login)
    {
        var search = connection.Search(
                _ldapSetting.Value.BaseSearchDistinguishedName,
                LdapConnection.ScopeSub,
                $"(samaccountname={login})",
                null,
                false);

        return search.FirstOrDefault();
    }

    /// <summary>
    /// Получение AD пользователя
    /// </summary>
    /// <param name="login"></param>   
    /// <returns></returns>
    private (bool, LdapEntry) TryGetLdapEntry(string login)
    {
        return _ldapConnectionFactory.WithAdminAuthConnection(connection =>
        {
            var entry = GetLdapEntryByLogin(connection, login);
            return entry == null ? (false, null) : (true, entry);
        });
    }   
}
