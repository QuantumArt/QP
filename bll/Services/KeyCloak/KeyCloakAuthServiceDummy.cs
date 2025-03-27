using System;
using System.Threading.Tasks;

namespace Quantumart.QP8.BLL.Services.KeyCloak;

public class KeyCloakAuthServiceDummy : ISsoAuthService
{
    private const string Disabled = "KeyCloak disabled in appsettings.json";

    public Task<SsoAuthResult> CheckUserAuth(string code, string verifier) => throw new NotImplementedException(Disabled);

    public string GenerateCodeVerifier(int length = 32) => throw new NotImplementedException(Disabled);

    public string GenerateCodeChallenge(string codeVerifier) => throw new NotImplementedException(Disabled);

    public string GetAuthenticateUrl(string state, string challenge) => throw new NotImplementedException(Disabled);

    public Task<bool> CheckSsoEnabled(string customerCode) => Task.FromResult(false);
}
