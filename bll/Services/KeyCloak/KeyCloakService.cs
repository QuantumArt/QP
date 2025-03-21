using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using NLog;
using Quantumart.QP8.Configuration;

namespace Quantumart.QP8.BLL.Services.KeyCloak;

public class KeyCloakService : IKeyCloakSyncService, IKeycloakAuthService
{
    private readonly IKeyCloakApiHelper _apiHelper;
    private readonly ILogger _logger = LogManager.GetCurrentClassLogger();
    private readonly KeyCloakSettings _settings;

    private const string GroupPathTemplate = "admin/realms/{0}/groups?search={1}";
    private const string UsersInGroupPathTemplate = "admin/realms/{0}/groups/{1}/members";

    public KeyCloakService(IKeyCloakApiHelper apiHelper, IOptions<KeyCloakSettings> settings)
    {
        _apiHelper = apiHelper;
        _settings = settings.Value;
    }

    public async Task<List<KeyCloakGroup>> GetGroups(List<string> groupNames)
    {
        List<KeyCloakGroup> result = new();

        foreach (string groupName in groupNames)
        {
            KeyCloakGroup[] groups = await _apiHelper.GetAsync<KeyCloakGroup[]>(string.Format(GroupPathTemplate, _settings.Realm, groupName));

            if (groups.Length == 0)
            {
                _logger.ForInfoEvent()
                    .Message("KeyCloak group not found.")
                    .Property("GroupName", groupName)
                    .Log();

                continue;
            }

            KeyCloakGroup foundGroup = RetrieveGroup(groups.First(), groupName);

            if (foundGroup == null)
            {
                continue;
            }

            result.Add(foundGroup);
        }

        return result;
    }

    public async Task<Dictionary<string, List<KeyCloakUser>>> GetUsers(List<KeyCloakGroup> keyCloakGroups)
    {
        Dictionary<string, List<KeyCloakUser>> result = new();

        foreach (KeyCloakGroup keyCloakGroup in keyCloakGroups)
        {
            KeyCloakUser[] users = await _apiHelper.GetAsync<KeyCloakUser[]>(string.Format(UsersInGroupPathTemplate, _settings.Realm, keyCloakGroup.Id));
            result.Add(keyCloakGroup.Name, users.ToList());
        }

        return result;
    }

    public Task<bool> CheckUserAuth(string code) => _apiHelper.CheckAuthorization(code);

    private static KeyCloakGroup RetrieveGroup(KeyCloakGroup group, string groupName)
    {
        KeyCloakGroup result = null;

        if (group.Name.Equals(groupName, StringComparison.OrdinalIgnoreCase))
        {
            group.SubGroups = Array.Empty<KeyCloakGroup>();
            result = group;

            return result;
        }

        foreach (KeyCloakGroup subGroup in group.SubGroups)
        {
            result = RetrieveGroup(subGroup, groupName);

            if (result != null)
            {
                return result;
            }
        }

        return result;
    }
}
