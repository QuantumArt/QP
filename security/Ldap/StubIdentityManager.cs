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

    public SignInResult PasswordSignIn(string login, string password)
    {
        throw new NotSupportedException("Stub method is used!");
    }

}
