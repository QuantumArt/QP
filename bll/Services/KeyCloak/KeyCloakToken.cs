using System;

namespace Quantumart.QP8.BLL.Services.KeyCloak;

public class KeyCloakToken
{
    public string Token { get; set; }
    public int Expiration { get; set; }
    private DateTime Created { get; set; } = DateTime.Now;

    public bool Expired => DateTime.Now > Created.AddSeconds(Expiration - 30);
}
