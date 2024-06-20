using System.Security.Cryptography;
using System.Text;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Repository.ArticleRepositories;
using Quantumart.QP8.BLL.Repository.Helpers;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Configuration;

namespace Quantumart.QP8.BLL.Services.DbServices
{
    public class DbService : IDbService
    {
        private readonly S3Options _options;

        public DbService(S3Options options)
        {
            _options = options;
        }

        public Db GetDbSettings() => DbRepository.Get();

        public static Db ReadSettings() => DbRepository.Get();

        public static Db ReadSettingsForUpdate() => DbRepository.GetForUpdate();

        public static Db UpdateSettings(Db db) => DbRepository.Update(db);

        public static string GetDbHash() => GehHash(GehHash(DbRepository.GetDbServerName()) + GehHash(DbRepository.GetDbName()));

        public static void ResetUserCache() => BackendActionCache.ResetForUser();

        public bool UseS3() => !string.IsNullOrWhiteSpace(_options.Endpoint) && DbRepository.Get().UseS3;

        public S3Options S3Options => _options;

        private static string GehHash(string value)
        {
            var data = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(value));
            var sBuilder = new StringBuilder();
            foreach (var v2bT in data)
            {
                sBuilder.Append(v2bT.ToString("x2"));
            }

            return sBuilder.ToString();
        }

        public static HomeResult Home() => new HomeResult
        {
            Sites = SiteService.GetSites(),
            CurrentUser = new UserService().ReadProperties(QPContext.CurrentUserId),
            LockedCount = ArticleRepository.GetLockedCount(),
            ApprovalCount = ArticleRepository.GetForApprovalCount()
        };
    }
}
