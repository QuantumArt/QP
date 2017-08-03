using System.Configuration;
using QP8.Infrastructure.Logging.Interfaces;
using Quantumart.QP8.BLL.Services.UserSynchronization;

namespace Quantumart.QP8.Scheduler.Users
{
    internal static class UserSynchronizationServiceFactory
    {
        private const string CurrentUserIdKey = "CurrentUserId";
        private const string UserLanguageIdKey = "UserLanguageId";

        public static UserSynchronizationService GetService(ILog logger)
        {
            if (!int.TryParse(ConfigurationManager.AppSettings[CurrentUserIdKey], out int currentUserId))
            {
                currentUserId = 1;
            }

            if (!int.TryParse(ConfigurationManager.AppSettings[UserLanguageIdKey], out int languageId))
            {
                languageId = 1;
            }

            return new UserSynchronizationService(logger, currentUserId, languageId);
        }
    }
}
