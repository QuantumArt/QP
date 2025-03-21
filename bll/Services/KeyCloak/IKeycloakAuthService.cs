using System.Threading.Tasks;

namespace Quantumart.QP8.BLL.Services.KeyCloak;

public interface IKeycloakAuthService
{
    Task<bool> CheckUserAuth(string code);
}
