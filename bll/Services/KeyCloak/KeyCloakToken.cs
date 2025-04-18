using System;

namespace Quantumart.QP8.BLL.Services.KeyCloak;

public class KeyCloakToken
{
    private const int TokenExpireSafeThreshold = 30;

    public string Token { get; set; }
    public int Expiration { get; set; }
    private DateTime Created { get; } = DateTime.Now;

    public bool Expired => DateTime.Now > Created.AddSeconds(Expiration - TokenExpireSafeThreshold);
}
