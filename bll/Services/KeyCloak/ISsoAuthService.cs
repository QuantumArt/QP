using System.Threading.Tasks;

namespace Quantumart.QP8.BLL.Services.KeyCloak;

public interface ISsoAuthService
{
    Task<SsoAuthResult> CheckUserAuth(string code, string verifier);

    string GenerateCodeVerifier(int length = 32);

    string GenerateCodeChallenge(string codeVerifier);

    string GetAuthenticateUrl(string state, string challenge);

    Task<bool> CheckSsoEnabled(string customerCode);
}
