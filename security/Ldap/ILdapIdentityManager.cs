using Novell.Directory.Ldap;
using System.Collections.Generic;

namespace Quantumart.QP8.Security.Ldap;

public interface ILdapIdentityManager
{
    string CurrentDomain
    {
        get;
    }

    /// <summary>
    /// Авторизация
    /// </summary>
    /// <param name="login"></param>
    /// <param name="password"></param>	
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    SignInResult PasswordSignIn(string login, string password);

    /// <summary>
    /// Load all groups from AD by filter with selected attributes
    /// </summary>
    /// <param name="filter">Ldap filter string to search with</param>
    /// <param name="attrsToSelect">Attributes array to fetch from AD</param>
    /// <returns></returns>
    List<LdapEntry> GetEntriesWithAttributesByFilter(string filter, string[] attrsToSelect);
}
