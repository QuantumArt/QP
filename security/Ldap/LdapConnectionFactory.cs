using Microsoft.Extensions.Options;
using Novell.Directory.Ldap;
using Quantumart.QP8.Configuration;
using System;
using System.Threading.Tasks;

namespace Quantumart.QP8.Security.Ldap;

public class LdapConnectionFactory
{
    private readonly LdapSettings _setting;
    private readonly LdapHelper _identityHelper;

    public LdapConnectionFactory(IOptions<LdapSettings> setting, LdapHelper identityHelper)
    {
        _identityHelper = identityHelper;
        _setting = setting.Value;
    }

    public T WithConnection<T>(
        Func<ILdapConnection, T> actionOnAuthenticatedLdapConnection)
    {
        using var connection = CreateConnection();
        return actionOnAuthenticatedLdapConnection(connection);
    }

    public void WithAdminAuthConnection(Func<ILdapConnection, Task> actionOnAuthenticatedLdapConnection)
    {
        using var connection = CreateAdminAuthConnection();
        actionOnAuthenticatedLdapConnection(connection);
    }

    public T WithAdminAuthConnection<T>(
        Func<ILdapConnection, T> actionOnAuthenticatedLdapConnection)
    {
        using var connection = CreateAdminAuthConnection();
        return actionOnAuthenticatedLdapConnection(connection);
    }

    /// <summary>
    /// Создание соединения к AD
    /// </summary>
    /// <returns></returns>
    private LdapConnection CreateConnection()
    {
        var ldapConnectionOptions = new LdapConnectionOptions()
            // TODO: Validate certificate properly.
            .ConfigureRemoteCertificateValidationCallback((sender, certificate, chain, errors) => true);
        var connection = new LdapConnection(ldapConnectionOptions)
        {
            SecureSocketLayer = _setting.UseSsl,
            ConnectionTimeout = _setting.ConnectionTimeout
        };
        connection.Connect(_setting.Server, GetServerPort());
        return connection;
    }

    private int GetServerPort()
    {
        if (_setting.Port > 0)
        {
            return _setting.Port;
        }

        return _setting.UseSsl
            ? LdapConnection.DefaultSslPort
            : LdapConnection.DefaultPort;
    }

    /// <summary>
    /// Создание коннекции под админской УЗ
    /// </summary>
    /// <returns></returns>
    private LdapConnection CreateAdminAuthConnection()
    {
        var connection = CreateConnection();
        var login = _identityHelper.BuildDomainName(_setting.AdminLogin);

        connection.Bind(login, _setting.AdminPassword);
        return connection;
    }
}
