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

    public async Task<T> WithConnection<T>(
        Func<ILdapConnection, Task<T>> actionOnAuthenticatedLdapConnection)
    {
        using var connection = await CreateConnection();
        return await actionOnAuthenticatedLdapConnection(connection);
    }

    public async Task WithAdminAuthConnection(Func<ILdapConnection, Task> actionOnAuthenticatedLdapConnection)
    {
        using var connection = await CreateAdminAuthConnection();
        await actionOnAuthenticatedLdapConnection(connection);
    }

    public async Task<T> WithAdminAuthConnection<T>(
        Func<ILdapConnection, Task<T>> actionOnAuthenticatedLdapConnection)
    {
        using var connection = await CreateAdminAuthConnection();
        return await actionOnAuthenticatedLdapConnection(connection);
    }

    /// <summary>
    /// Создание соединения к AD
    /// </summary>
    /// <returns></returns>
    private async Task<LdapConnection> CreateConnection()
    {
        var ldapConnectionOptions = new LdapConnectionOptions()
            // TODO: Validate certificate properly.
            .ConfigureRemoteCertificateValidationCallback((sender, certificate, chain, errors) => true);
        var connection = new LdapConnection(ldapConnectionOptions)
        {
            SecureSocketLayer = _setting.UseSsl
        };
        await connection.ConnectAsync(_setting.Server, GetServerPort());
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
    private async Task<LdapConnection> CreateAdminAuthConnection()
    {
        var connection = await CreateConnection();
        var login = _identityHelper.BuildDomainName(_setting.AdminLogin);

        await connection.BindAsync(login, _setting.AdminPassword);
        return connection;
    }
}
