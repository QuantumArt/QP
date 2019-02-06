using System.Collections.Generic;
using System.Linq;
using Quantumart.QP8.BLL.Facades;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.DAL;

namespace Quantumart.QP8.BLL.Repository
{
    public class DbRepository
    {
        public static Db Get()
        {
            var result = MapperFacade.DbMapper.GetBizObject(QPContext.EFContext.DbSet.Include("LastModifiedByUser").FirstOrDefault());
            result.AppSettings = GetAppSettings();
            return result;
        }

        public static Db GetForUpdate()
        {
            var result = MapperFacade.DbMapper.GetBizObject(QPContext.EFContext.DbSet.FirstOrDefault());
            result.AppSettings = GetAppSettings();
            return result;
        }

        public static Db Update(Db db)
        {
            var result = DefaultRepository.Update<Db, DbDAL>(db);
            DeleteAppSettings();
            SaveAppSettings(db.AppSettings);
            result.AppSettings = db.AppSettings;
            return result;
        }

        public static bool RecordActions => Get().RecordActions;

        public static string GetDbName()
        {
            using (var scope = new QPConnectionScope())
            {
                return Common.GetDbName(scope.DbConnection);
            }
        }

        public static string GetDbServerName()
        {
            using (var scope = new QPConnectionScope())
            {
                return Common.GetDbServerName(scope.DbConnection);
            }
        }

        public static IEnumerable<AppSettingsItem> GetAppSettings()
        {
            return QPContext.EFContext.AppSettingsSet.Select(n => new AppSettingsItem { Key = n.Key, Value = n.Value }).ToList();
        }

        internal static void SaveAppSettings(IEnumerable<AppSettingsItem> values)
        {
            var result = values.Select(n => new AppSettingsDAL(){ Key = n.Key, Value = n.Value} ).ToList();
            DefaultRepository.SimpleSaveBulk(result);
        }

        internal static void DeleteAppSettings()
        {
            var context = QPContext.EFContext;
            DefaultRepository.SimpleDeleteBulk(context.AppSettingsSet.ToList(), context);
        }
    }
}
