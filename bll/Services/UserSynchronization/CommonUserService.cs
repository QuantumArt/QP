using System;
using System.Linq;
using System.Threading;
using NLog;
using Quantumart.QP8.BLL.Repository;

namespace Quantumart.QP8.BLL.Services.UserSynchronization
{
    public class CommonUserService : ICommonUserService
    {
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private readonly int _currentUserId;

        public CommonUserService()
        {
           _currentUserId = 1;
        }

        public void DisableUsers(string customerName, int diff,  CancellationToken token)
        {
            QPContext.CurrentUserId = _currentUserId;
            var currentDate = DateTime.Now;
            _logger.Info($"Start processing users on customer code {customerName}");

            var allUsers = UserRepository.GetAllUsersListWithGroups().Where(w=>!w.Disabled);

            if (!allUsers.Any())
            {
                _logger.Info($"There are no active users on customer code {customerName}");
                return;
            }

            var inactiveUsers = allUsers.Where(w=> w.LastLogOn.HasValue).Where(w => (currentDate - w.LastLogOn.Value).TotalDays > diff);
            var nonActivatedUsers = allUsers.Where(w => !w.LastLogOn.HasValue).Where(s => (currentDate - s.Created).TotalDays > diff);

            var qpUsersToBeDisabled = inactiveUsers.Union(nonActivatedUsers);

            if (!qpUsersToBeDisabled.Any())
            {
                _logger.Info($"All inactive users is already disabled  on customer code {customerName}");
                return;
            }

            foreach (var qpUser in qpUsersToBeDisabled)
            {
                if (!token.IsCancellationRequested)
                {
                    try
                    {
                        qpUser.Disabled = true;
                        UserRepository.UpdateProperties(qpUser);
                        _logger.Info($"User {qpUser.Name} was disabled because user has been inactive for {diff} days on customer code {customerName}");
                    }
                    catch (Exception ex)
                    {
                        _logger.Warn(ex, $"There was an exception while disabling user on customer code {customerName}", ex);
                    }
                }
                else
                {
                    _logger.Info($"Enabling was interrupted on user {qpUser.Name}");
                    break;
                }
            }
        }

        public void EnableUsers(string customerName, string[] excludedUsers, CancellationToken token)
        {
            QPContext.CurrentUserId = _currentUserId;
            var qpUsersToBeDisabled = UserRepository.GetAllUsersListWithGroups().Where(w=>w.Disabled);


            if (excludedUsers != null)
            {
                qpUsersToBeDisabled = qpUsersToBeDisabled.Where(w=>!excludedUsers.Contains(w.Name, StringComparer.InvariantCultureIgnoreCase));
            }

            if (!qpUsersToBeDisabled.Any())
            {
                _logger.Info($"There are no inactive users on customer code {customerName}");
                return;
            }

            foreach (var qpUser in qpUsersToBeDisabled)
            {
                if (!token.IsCancellationRequested)
                {
                    try
                    {
                        qpUser.Disabled = false;
                        UserRepository.UpdateProperties(qpUser);
                        _logger.Info($"User {qpUser.Name} was enabled on customer code {customerName}");
                    }
                    catch (Exception ex)
                    {
                        _logger.Warn(ex, $"There was an exception while enabling user on customer code {customerName}", ex);
                    }
                }
                else
                {
                    _logger.Info($"Disabling was interrupted on user {qpUser.Name}");
                    break;
                }
            }
        }
    }
}
