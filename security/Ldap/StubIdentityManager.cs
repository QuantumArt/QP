using System;
using System.Threading;
using System.Threading.Tasks;

namespace Quantumart.QP8.Security.Ldap;

public class StubIdentityManager : ILdapIdentityManager
{
    public string CurrentDomain
    {
        get { throw new NotSupportedException("Stub method is used!"); }
    }
    
    public async Task<SignInResult> PasswordSignIn(
        string login,
        string password,       
        CancellationToken cancellationToken)
    {
        throw new NotSupportedException("Stub method is used!");
    }
     
}
