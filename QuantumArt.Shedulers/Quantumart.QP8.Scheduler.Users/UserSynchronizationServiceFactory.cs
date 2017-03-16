using System.Configuration;
using System.Diagnostics;
using Quantumart.QP8.BLL.Services.UserSynchronization;

namespace Quantumart.QP8.Scheduler.Users
{
    internal static class UserSynchronizationServiceFactory
    {
        private const string CurrentUserIdKey = "CurrentUserId";
        private const string UserLanguageIdKey = "UserLanguageId";

        public static UserSynchronizationService GetService(TraceSource logger)
        {
            int currentUserId;
            if (!int.TryParse(ConfigurationManager.AppSettings[CurrentUserIdKey], out currentUserId))
            {
                currentUserId = 1;
            }

            int languageId;
            if (!int.TryParse(ConfigurationManager.AppSettings[UserLanguageIdKey], out languageId))
            {
                languageId = 1;
            }

            return new UserSynchronizationService(currentUserId, languageId, logger);
        }
    }
}
