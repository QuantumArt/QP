using System.Linq;
using Quantumart.QP8.BLL.Repository;
using System;
using Quantumart.QP8.BLL.Repository.ActiveDirectory;
using System.Collections.Generic;
using System.Diagnostics;

namespace Quantumart.QP8.BLL.Services.UserSynchronization
{
	public class UserSynchronizationService : IUserSynchronizationService
	{
		#region Constants
		private const string DefaultMail = "undefined@domain.com";
		private const string DefaultValue = "undefined";
		#endregion

		#region Private fields
		private readonly int _languageId;
		private readonly TraceSource _logger;
		private readonly ActiveDirectoryRepository _activeDirectory;
		#endregion

		#region Constructor
		public UserSynchronizationService(int currentUserId, int languageId, TraceSource logger)
		{
			QPContext.CurrentUserId = currentUserId;
			_languageId = languageId;
			_logger = logger;
			_activeDirectory = new ActiveDirectoryRepository();
		}
		#endregion

		#region IUserSynchronizationService implementation
		public bool NeedSynchronization()
		{
			return DbRepository.Get().UseADSyncService;
		}		

		public void Synchronize()
		{
			#region Prepare data
			var qpGroups = UserGroupRepository.GetNtGroups();
			var qpGroupNames = qpGroups.Select(g => g.NtGroup).ToArray();
			var qpUsers = UserRepository.GetNtUsers();

			var adGroups = _activeDirectory.GetGroups(qpGroupNames);
			var adGroupNames = adGroups.Select(g => g.Name).ToArray();			
			#endregion

			#region Validate data
			var missedADGroups = qpGroupNames.Except(adGroupNames).ToArray();

			if (missedADGroups.Any())
			{
				_logger.TraceEvent(TraceEventType.Warning, 0, "Group(s) \"{0}\" is(are) missed in Active Directory", string.Join(", ", missedADGroups) );
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
										 where qpg.ParentGroup == null || adRelation.Members.Any(m => qpg.ParentGroup.NtGroup == m)
										 select adRelation.Group).ToArray();

			var adUsers = _activeDirectory.GetUsers(adGroupsToBeProcessed);

			var wrongMembershipADGroups = adGroupNames.Except(adGroupsToBeProcessed.Select(g => g.Name)).ToArray();

			if (wrongMembershipADGroups.Any())
			{
				_logger.TraceEvent(TraceEventType.Warning, 0, "Group(s) \"{0}\" have wrong membership", string.Join(", ", wrongMembershipADGroups));
			}



			#endregion

			#region Add users
			var adUsersToBeAdded = adUsers.Where(adu => !adu.IsDisabled && !qpUsers.Any(qpu => adu.AccountName == qpu.NTLogOn));
		
			foreach (var adUser in adUsersToBeAdded)
			{
				try
				{
					var qpUser = CreateUser(adUser);
					MapUser(adUser, ref qpUser);
					MapGroups(adUser, ref qpUser, adGroupsToBeProcessed, qpGroups);
					UserRepository.SaveProperties(qpUser);
					_logger.TraceEvent(TraceEventType.Verbose, 0, "user {0} is added", qpUser.DisplayName);
				}
				catch (Exception ex)
				{
					_logger.TraceData(TraceEventType.Warning, 0, ex);
				}
			}
			#endregion

			#region Update users
			var usersToBeUpdated = from qpu in qpUsers
								   join adu in adUsers on qpu.NTLogOn equals adu.AccountName
								   select new { QP = qpu, AD = adu };

			foreach (var user in usersToBeUpdated)
			{
				try
				{
					var qpUser = user.QP;
					MapUser(user.AD, ref qpUser);
					MapGroups(user.AD, ref qpUser, adGroupsToBeProcessed, qpGroups);
					UserRepository.UpdateProperties(qpUser);
					_logger.TraceEvent(TraceEventType.Verbose, 0, "user {0} is updated", qpUser.DisplayName);
				}
				catch (Exception ex)
				{
					_logger.TraceData(TraceEventType.Warning, 0, ex);
				}
			}
			#endregion

			#region Disable users
			var qpUsersToBeDisabled = qpUsers.Where(qpu => !adUsers.Any(adu => adu.AccountName == qpu.NTLogOn));

			foreach (var qpUser in qpUsersToBeDisabled)
			{
				try
				{
					qpUser.Disabled = true;
					UserRepository.UpdateProperties(qpUser);
					_logger.TraceEvent(TraceEventType.Verbose, 0, "user {0} is disabled", qpUser.DisplayName);
				}
				catch (Exception ex)
				{
					_logger.TraceData(TraceEventType.Warning, 0, ex);
				}
			}
			#endregion
		}
		#endregion

		#region Private methods
		private User CreateUser(ActiveDirectoryUser user)
		{
			return new User
			{
				LogOn = user.AccountName,
				NTLogOn = user.AccountName,
				Password = UserRepository.GeneratePassword(),
				LanguageId = _languageId,
				AutoLogOn = true,
				Groups = new UserGroup[0]
			};
		}

		private void MapUser(ActiveDirectoryUser adUser, ref User qpUser)
		{
			qpUser.FirstName = adUser.FirstName ?? DefaultValue;
			qpUser.LastName = adUser.LastName ?? DefaultValue;
			qpUser.Email = adUser.Mail ?? DefaultMail;
			qpUser.Disabled = adUser.IsDisabled;
		}

		private void MapGroups(ActiveDirectoryUser adUser, ref User qpUser, IEnumerable<ActiveDirectoryGroup> adGroups, IEnumerable<UserGroup> qpGroups)
		{
			var importedGroups = (from qpg in qpGroups
								 join adg in adGroups on qpg.NtGroup equals adg.Name
								 where adUser.MemberOf.Contains(adg.ReferencedPath)
								 select qpg);

			var nativeGroups = qpUser.Groups.Except(qpGroups);

			qpUser.Groups = importedGroups.Concat(nativeGroups);
		}
		#endregion	
	}
}