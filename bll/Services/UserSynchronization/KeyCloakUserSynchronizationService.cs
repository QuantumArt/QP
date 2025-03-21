using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using NLog;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Services.API;
using Quantumart.QP8.BLL.Services.KeyCloak;

namespace Quantumart.QP8.BLL.Services.UserSynchronization;

public class KeyCloakUserSynchronizationService : UserSynchronizationServiceBase
{
    private readonly IKeyCloakSyncService _keyCloakSyncService;

    public KeyCloakUserSynchronizationService(IOptions<CommonSchedulerProperties> options, IKeyCloakSyncService keyCloakSyncService)
        : base(options, LogManager.GetCurrentClassLogger())
    {
        _keyCloakSyncService = keyCloakSyncService;
    }

    public override bool NeedSynchronization() => DbRepository.Get().UseAdSyncService;

    public override void Synchronize()
    {
        QPContext.CurrentUserId = Settings.DefaultUserId;
        List<UserGroup> qpGroups = UserGroupRepository.GetNtGroups().ToList();

        if (qpGroups.Count == 0)
        {
            Logger.Info("QP groups not found");
            return;
        }

        List<KeyCloakGroup> kcGroups = _keyCloakSyncService.GetGroups(qpGroups.Select(x => x.NtGroup).ToList()).Result;
        Dictionary<string, List<KeyCloakUser>> kcGroupsWithUsers = new();

        if (kcGroups.Count != 0)
        {
            kcGroupsWithUsers = _keyCloakSyncService.GetUsers(kcGroups).Result;
        }

        List<User> qpUsers = UserRepository.GetNtUsers();

        List<KeyCloakUser> kcNormalized = NormalizeUsers(kcGroupsWithUsers);

        Logger.ForTraceEvent()
            .Message("Found users and groups")
            .Property("qpGroups", qpGroups.Select(g => g.Id).ToArray())
            .Property("qpUsers", qpUsers.Select(u => u.Id).ToArray())
            .Property("kcGroups", kcGroups.Select(g => g.Name).ToArray())
            .Property("kcUsers", kcNormalized.Select(u => u.UserName).ToArray())
            .Log();

        AddUsers(qpUsers, qpGroups, kcNormalized);
        UpdateUsers(qpUsers, qpGroups, kcNormalized);
        DisableUsers(qpUsers, kcNormalized);
    }

    private List<KeyCloakUser> NormalizeUsers(Dictionary<string, List<KeyCloakUser>> kcGroupsWithUsers)
    {
        List<KeyCloakUser> result = new();

        foreach (KeyValuePair<string,List<KeyCloakUser>> groupWithUsers in kcGroupsWithUsers)
        {
            foreach (KeyCloakUser keyCloakUser in groupWithUsers.Value)
            {
                KeyCloakUser user = result.FirstOrDefault(x => x.Id == keyCloakUser.Id);

                if (user != null && user.Groups.All(x => x != groupWithUsers.Key))
                {
                    user.Groups.Add(groupWithUsers.Key);
                }
                else
                {
                    keyCloakUser.Groups.Add(groupWithUsers.Key);
                    result.Add(keyCloakUser);
                }
            }
        }

        return result;
    }

    private void AddUsers(List<User> qpUsers, List<UserGroup> qpGroups, List<KeyCloakUser> kcUsersWithGroups)
    {
        List<KeyCloakUser> usersToAdd = kcUsersWithGroups.Where(kcu => kcu.Enabled && qpUsers.All(qpu => qpu.NtLogOn != kcu.UserName)).ToList();

        foreach (KeyCloakUser userToAdd in usersToAdd)
        {
            try
            {
                User qpUser = CreateUser(userToAdd);
                MapUser(userToAdd, ref qpUser);
                MapGroups(ref qpUser, qpGroups, userToAdd.Groups);
                UserRepository.SaveProperties(qpUser);
                Logger.Info($"User {qpUser.DisplayName} from groups {string.Join(",", GetGroupNames(qpUser))} was added");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "There was an exception while adding user");
            }
        }
    }

    private void UpdateUsers(IEnumerable<User> qpUsers, List<UserGroup> qpGroups, List<KeyCloakUser> kcUsersWithGroups)
    {
        var usersToBeUpdated = from qpu in qpUsers
                               join kcu in kcUsersWithGroups on qpu.NtLogOn equals kcu.UserName
                               select new { QP = qpu, KC = kcu };

        foreach (var user in usersToBeUpdated)
        {
            try
            {
                User qpUser = user.QP;
                MapUser(user.KC, ref qpUser);
                MapGroups(ref qpUser, qpGroups, user.KC.Groups);
                UserRepository.UpdateProperties(qpUser);
                Logger.Info($"User {qpUser.DisplayName} from groups {string.Join(",", GetGroupNames(qpUser))} was updated");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "There was an exception while updating user");
            }
        }
    }

    private void DisableUsers(IEnumerable<User> qpUsers, List<KeyCloakUser> kcUsers)
    {
        IEnumerable<User> qpUsersToBeDisabled = qpUsers.Where(qpu => kcUsers.All(kcu => kcu.UserName != qpu.NtLogOn));
        foreach (User qpUser in qpUsersToBeDisabled)
        {
            try
            {
                qpUser.Disabled = true;
                UserRepository.UpdateProperties(qpUser);
                Logger.Info($"User {qpUser.DisplayName} was disabled");
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, "There was an exception while disabling user");
            }
        }
    }

    private User CreateUser(KeyCloakUser user) => new()
    {
        LogOn = user.UserName,
        NtLogOn = user.UserName,
        Password = UserRepository.GeneratePassword(),
        LanguageId = Settings.DefaultLanguageId,
        AutoLogOn = true,
        Groups = Array.Empty<UserGroup>()
    };

    private static void MapUser(KeyCloakUser ckUser, ref User qpUser)
    {
        qpUser.FirstName = ckUser.FirstName ?? DefaultValue;
        qpUser.LastName = ckUser.LastName ?? DefaultValue;
        qpUser.Email = ckUser.Email ?? DefaultMail;
        qpUser.Disabled = !ckUser.Enabled;
    }

    private static void MapGroups(ref User qpUser, List<UserGroup> qpGroups, List<string> kcGroups)
    {
        IEnumerable<UserGroup> importedGroups = from qpg in qpGroups
                                                join kcg in kcGroups on qpg.NtGroup equals kcg
                                                select qpg;

        IEnumerable<UserGroup> nativeGroups = qpUser.Groups.Except(qpGroups);
        qpUser.Groups = importedGroups.Concat(nativeGroups);
    }

    private static string[] GetGroupNames(User qpUser)
    {
        return qpUser.Groups?.Select(g => g.Name).ToArray() ?? Array.Empty<string>();
    }
}
