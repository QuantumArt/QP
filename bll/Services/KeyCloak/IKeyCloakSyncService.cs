using System.Collections.Generic;
using System.Threading.Tasks;

namespace Quantumart.QP8.BLL.Services.KeyCloak;

public interface IKeyCloakSyncService
{
    Task<List<KeyCloakGroup>> GetGroups(List<string> groupNames);

    Task<Dictionary<string, List<KeyCloakUser>>> GetUsers(List<KeyCloakGroup> keyCloakGroups);
}
