using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using NLog;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Repository.ActiveDirectory;
using Quantumart.QP8.BLL.Services.API;

namespace Quantumart.QP8.BLL.Services.UserSynchronization
{
    public class UserSynchronizationService : UserSynchronizationServiceBase
    {
        private readonly IActiveDirectoryRepository _activeDirectory;

        public UserSynchronizationService(IOptions<CommonSchedulerProperties> options, IActiveDirectoryRepository activeDirectoryRepository)
        : base(options, LogManager.GetCurrentClassLogger())
        {
            _activeDirectory = activeDirectoryRepository;
        }

        public override bool NeedSynchronization() => DbRepository.Get().UseAdSyncService;

        public override void Synchronize()
        {
            QPContext.CurrentUserId = Settings.DefaultUserId;
            var qpGroups = UserGroupRepository.GetNtGroups().ToList();

            if (qpGroups.Count == 0)
            {
                Logger.Info($"QP groups not found");
                return;
            }

            var adGroups = GetAdGroupsToProcess(qpGroups);
            var adUsers = Array.Empty<ActiveDirectoryUser>();

            if (adGroups.Length != 0)
            {
                adUsers = _activeDirectory.GetUsers(adGroups);
            }

            var qpUsers = UserRepository.GetNtUsers();

            Logger.ForTraceEvent()
               .Message("Found users and groups")
               .Property("qpGroups", qpGroups.Select(g => g.Id).ToArray())
               .Property("qpUsers", qpUsers.Select(u => u.Id).ToArray())
               .Property("adGroups", adGroups.Select(g => g.ReferencedPath).ToArray())
               .Property("adUsers", adUsers.Select(u => u.ReferencedPath).ToArray())
               .Log();

            AddUsers(adUsers, adGroups, qpUsers, qpGroups);
            UpdateUsers(qpUsers, adUsers, adGroups, qpGroups);
            DisableUsers(qpUsers, adUsers);
        }

        private void AddUsers(IEnumerable<ActiveDirectoryUser> adUsers, ActiveDirectoryGroup[] adGroups, List<User> qpUsers, List<UserGroup> qpGroups)
        {
            var adUsersToAdd = adUsers.Where(adu => !adu.IsDisabled && qpUsers.All(qpu => adu.AccountName != qpu.NtLogOn));
            foreach (var adUser in adUsersToAdd)
            {
                try
                {
                    var qpUser = CreateUser(adUser);
                    MapUser(adUser, ref qpUser);
                    MapGroups(adUser, ref qpUser, adGroups, qpGroups);
                    UserRepository.SaveProperties(qpUser);
                    Logger.Info($"User {qpUser.DisplayName} from groups {string.Join(",", GetGroupNames(qpUser))} was added");
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "There was an exception while adding user");
                }
            }
        }

        private void UpdateUsers(IEnumerable<User> qpUsers, IEnumerable<ActiveDirectoryUser> adUsers, ActiveDirectoryGroup[] adGroups, List<UserGroup> qpGroups)
        {
            var usersToBeUpdated = from qpu in qpUsers
                                   join adu in adUsers on qpu.NtLogOn equals adu.AccountName
                                   select new { QP = qpu, AD = adu };

            foreach (var user in usersToBeUpdated)
            {
                try
                {
                    var qpUser = user.QP;
                    MapUser(user.AD, ref qpUser);
                    MapGroups(user.AD, ref qpUser, adGroups, qpGroups);
                    UserRepository.UpdateProperties(qpUser);
                    Logger.Info($"User {qpUser.DisplayName} from groups {string.Join(",", GetGroupNames(qpUser))} was updated");
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "There was an exception while updating user");
                }
            }
        }

        private void DisableUsers(IEnumerable<User> qpUsers, ActiveDirectoryUser[] adUsers)
        {
            var qpUsersToBeDisabled = qpUsers.Where(qpu => adUsers.All(adu => adu.AccountName != qpu.NtLogOn));
            foreach (var qpUser in qpUsersToBeDisabled)
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

        private ActiveDirectoryGroup[] GetAdGroupsToProcess(List<UserGroup> qpGroups)
        {
            var qpGroupNames = qpGroups.Select(g => g.NtGroup).ToArray();
            var adGroups = _activeDirectory.GetGroups(qpGroupNames);
            var adGroupNames = adGroups.Select(g => g.Name).ToArray();

            var missedAdGroups = qpGroupNames.Except(adGroupNames).ToArray();
            if (missedAdGroups.Any())
            {
                Logger.Warn($"Groups \"{string.Join(", ", missedAdGroups)}\" not exist at active directory");
            }

            var adGroupRelations = from adg in adGroups
                                   select new
                                   {
                                       Group = adg,
                                       Members = from m in adg.MemberOf
                                                 join g in adGroups on m equals g.ReferencedPath
                                                 select g.Name
                                   };

            var adGroupsToProcess = (from adRelation in adGroupRelations
                                     join qpg in qpGroups on adRelation.Group.Name equals qpg.NtGroup
                                     where qpg.ParentGroup == null || qpGroups.All(g => g.Id != qpg.ParentGroup.Id) || adRelation.Members.Any(m => qpg.ParentGroup.NtGroup == m)
                                     select adRelation.Group)
                .ToArray();

            var wrongMembershipAdGroups = adGroupNames.Except(adGroupsToProcess.Select(g => g.Name)).ToArray();
            if (wrongMembershipAdGroups.Any())
            {
                Logger.Warn($"Group(s) \"{string.Join(", ", wrongMembershipAdGroups)}\" have wrong membership");
            }

            return adGroupsToProcess;
        }

        private User CreateUser(ActiveDirectoryUser user) => new User
        {
            LogOn = user.AccountName,
            NtLogOn = user.AccountName,
            Password = UserRepository.GeneratePassword(),
            LanguageId = Settings.DefaultLanguageId,
            AutoLogOn = true,
            Groups = Array.Empty<UserGroup>()
        };

        private static void MapUser(ActiveDirectoryUser adUser, ref User qpUser)
        {
            qpUser.FirstName = adUser.FirstName ?? DefaultValue;
            qpUser.LastName = adUser.LastName ?? DefaultValue;
            qpUser.Email = adUser.Mail ?? DefaultMail;
            qpUser.Disabled = adUser.IsDisabled;
        }

        private static void MapGroups(ActiveDirectoryEntityBase adUser, ref User qpUser, IEnumerable<ActiveDirectoryGroup> adGroups, List<UserGroup> qpGroups)
        {
            var importedGroups = from qpg in qpGroups
                                 join adg in adGroups on qpg.NtGroup equals adg.Name
                                 where adUser.MemberOf.Contains(adg.ReferencedPath)
                                 select qpg;

            var nativeGroups = qpUser.Groups.Except(qpGroups);
            qpUser.Groups = importedGroups.Concat(nativeGroups);
        }

        private static string[] GetGroupNames(User qpUser)
        {
            return qpUser.Groups?.Select(g => g.Name).ToArray() ?? new string[0];
        }
    }
}
