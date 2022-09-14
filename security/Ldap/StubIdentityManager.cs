using System;
using System.Threading;
using System.Threading.Tasks;

namespace Quantumart.QP8.Security.Ldap;

public class StubIdentityManager : ILdapIdentityManager
{
    /// <summary>
    /// Indicates that the account never expires
    /// </summary>
    /// <remarks>
    /// See https://ldapwiki.com/wiki/AccountExpires
    /// </remarks>
    private static readonly long[] AccountNeverExpiresTicks = { 0, 9223372036854775807 };    

    public string CurrentDomain
    {
        get { return string.Empty; }
    }
    
    public async Task<SignInResult> PasswordSignIn(
        string login,
        string password,       
        CancellationToken cancellationToken)
    {
        throw new NotSupportedException("Stub method is used!");
    }
     
}
