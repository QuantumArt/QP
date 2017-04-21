using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Repository.ActiveDirectory;

namespace Quantumart.QP8.BLL.Services.UserSynchronization
{
    public class UserSynchronizationService : IUserSynchronizationService
    {
        private const string DefaultMail = "undefined@domain.com";
        private const string DefaultValue = "undefined";

        private readonly int _languageId;
        private readonly TraceSource _logger;
        private readonly ActiveDirectoryRepository _activeDirectory;

        public UserSynchronizationService(int currentUserId, int languageId, TraceSource logger)
        {
            QPContext.CurrentUserId = currentUserId;
            _languageId = languageId;
            _logger = logger;
            _activeDirectory = new ActiveDirectoryRepository();
        }

        public bool NeedSynchronization()
        {
            return DbRepository.Get().UseAdSyncService;
        }

        public void Synchronize()
        {
            // Prepare data
            var qpGroups = UserGroupRepository.GetNtGroups().ToList();
            var qpGroupNames = qpGroups.Select(g => g.NtGroup).ToArray();
            var adGroups = _activeDirectory.GetGroups(qpGroupNames);
            var adGroupNames = adGroups.Select(g => g.Name).ToArray();

            // Validate data
            var missedAdGroups = qpGroupNames.Except(adGroupNames).ToArray();
            if (missedAdGroups.Any())
            {
                _logger.TraceEvent(TraceEventType.Warning, 0, $"Group(s) \"{string.Join(", ", missedAdGroups)}\" is(are) missed in Active Directory");
            }

            var adGroupRelations = from adg in adGroups
                                   select new
                                   {
                                       Group = adg,
                                       Members = from m in adg.MemberOf
                                                 join g in adGroups on m equals g.ReferencedPath
                                                 select g.Name
                                   };

            var adGroupsToBeProcessed = (from adRelation in adGroupRelations
                                         join qpg in qpGroups on adRelation.Group.Name equals qpg.NtGroup
                                         where qpg.ParentGroup == null || qpGroups.All(g => g.Id != qpg.ParentGroup.Id) || adRelation.Members.Any(m => qpg.ParentGroup.NtGroup == m)
                                         select adRelation.Group)
                                         .ToArray();

            var adUsers = _activeDirectory.GetUsers(adGroupsToBeProcessed);
            var wrongMembershipAdGroups = adGroupNames.Except(adGroupsToBeProcessed.Select(g => g.Name)).ToArray();
            if (wrongMembershipAdGroups.Any())
            {
                _logger.TraceEvent(TraceEventType.Warning, 0, $"Group(s) \"{string.Join(", ", wrongMembershipAdGroups)}\" have wrong membership");
            }

            // Add users
            var qpUsers = UserRepository.GetNtUsers();
            var adUsersToBeAdded = adUsers.Where(adu => !adu.IsDisabled && qpUsers.All(qpu => adu.AccountName != qpu.NtLogOn));
            foreach (var adUser in adUsersToBeAdded)
            {
                try
                {
                    var qpUser = CreateUser(adUser);
                    MapUser(adUser, ref qpUser);
                    MapGroups(adUser, ref qpUser, adGroupsToBeProcessed, qpGroups);
                    UserRepository.SaveProperties(qpUser);
                    _logger.TraceEvent(TraceEventType.Verbose, 0, $"user {qpUser.DisplayName} in groups {string.Join(",", GetGroupNames(qpUser))} is added");
                }
                catch (Exception ex)
                {
                    _logger.TraceData(TraceEventType.Warning, 0, ex);
                }
            }

            // Update users
            var usersToBeUpdated = from qpu in qpUsers
                                   join adu in adUsers on qpu.NtLogOn equals adu.AccountName
                                   select new { QP = qpu, AD = adu };

            foreach (var user in usersToBeUpdated)
            {
                try
                {
                    var qpUser = user.QP;
                    MapUser(user.AD, ref qpUser);
                    MapGroups(user.AD, ref qpUser, adGroupsToBeProcessed, qpGroups);
                    UserRepository.UpdateProperties(qpUser);
                    _logger.TraceEvent(TraceEventType.Verbose, 0, $"user {qpUser.DisplayName} in groups {string.Join(",", GetGroupNames(qpUser))} is updated");
                }
                catch (Exception ex)
                {
                    _logger.TraceData(TraceEventType.Warning, 0, ex);
                }
            }

            // Disable users
            var qpUsersToBeDisabled = qpUsers.Where(qpu => adUsers.All(adu => adu.AccountName != qpu.NtLogOn));
            foreach (var qpUser in qpUsersToBeDisabled)
            {
                try
                {
                    qpUser.Disabled = true;
                    UserRepository.UpdateProperties(qpUser);
                    _logger.TraceEvent(TraceEventType.Verbose, 0, $"user {qpUser.DisplayName} is disabled");
                }
                catch (Exception ex)
                {
                    _logger.TraceData(TraceEventType.Warning, 0, ex);
                }
            }
        }

        private User CreateUser(ActiveDirectoryUser user)
        {
            return new User
            {
                LogOn = user.AccountName,
                NtLogOn = user.AccountName,
                Password = UserRepository.GeneratePassword(),
                LanguageId = _languageId,
                AutoLogOn = true,
                Groups = new UserGroup[0]
            };
        }

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
