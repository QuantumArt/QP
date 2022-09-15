using System.Threading;
using System.Threading.Tasks;

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
}
