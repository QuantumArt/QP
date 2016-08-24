using Quantumart.QP8.BLL.Services.UserSynchronization;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quantumart.QP8.Scheduler.Users
{
	internal static class UserSynchronizationServiceFactory
	{
		private const string CurrentUserIdKey = "CurrentUserId";
		private const string UserLanguageIdKey = "UserLanguageId";

		public static UserSynchronizationService GetService(TraceSource logger)
		{
			int currentUserId;
			int languageId;

			if (!int.TryParse(ConfigurationManager.AppSettings[CurrentUserIdKey], out currentUserId))
			{
				currentUserId = 1;
			}

			if (!int.TryParse(ConfigurationManager.AppSettings[UserLanguageIdKey], out languageId))
			{
				languageId = 1;
			}

			return new UserSynchronizationService(currentUserId, languageId, logger);
		}
	}
}