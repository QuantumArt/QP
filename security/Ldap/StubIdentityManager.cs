using System;
using System.Collections.Generic;
using Novell.Directory.Ldap;

namespace Quantumart.QP8.Security.Ldap;

public class StubIdentityManager : ILdapIdentityManager
{
    public string CurrentDomain
    {
        get { throw new NotSupportedException("Stub method is used!"); }
    }

    public List<LdapEntry> GetEntriesWithAttributesByFilter(string filter, string[] attrsToSelect)
    {
        throw new NotImplementedException("Stub method is used!");
    }

    public SignInResult PasswordSignIn(string login, string password)
    {
        throw new NotSupportedException("Stub method is used!");
    }

}
