using System;
using System.ComponentModel.DataAnnotations;

namespace Quantumart.QP8.Security.Ldap;

public class LdapSettings
{
    [Required]
    public string Server { get; init; } = default!;

    public bool UseSsl { get; init; }

    public int Port { get; init; }

    [Required]
    public string BaseSearchDistinguishedName { get; init; } = default!;

    [Required]
    public string Domain { get; init; } = default!;

    [Required]
    public string AdminLogin { get; init; } = default!;

    [Required]
    public string AdminPassword { get; init; } = default!;
    /// <summary>
    /// Connection timeout in milliseconds
    /// </summary>
    public TimeSpan ConnectionTimeout { get; init; } // default is 0 which will use the platform default timeout for TCP connections
}
