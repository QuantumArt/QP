using System.Threading.Tasks;

namespace Quantumart.QP8.BLL.Services.KeyCloak;

public interface IKeyCloakApiHelper
{
    Task<T> GetAsync<T>(string apiUrl);

    Task<bool> CheckAuthorization(string code, string verifier);
}
