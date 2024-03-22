using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Quantumart.QP8.BLL.Facades;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.DAL;
using Quantumart.QP8.DAL.Entities;

namespace Quantumart.QP8.BLL.Repository
{
    public class DbRepository
    {
        public static Db Get()
        {
            var result = MapperFacade.DbMapper.GetBizObject(
                QPContext.EFContext.DbSet
                    .Include(x => x.LastModifiedByUser)
                    .OrderBy(n => n.Id).FirstOrDefault()
            );
            result.AppSettings = GetAppSettings();
            return result;
        }

        public static Db GetForUpdate()
        {
            var result = MapperFacade.DbMapper.GetBizObject(
                QPContext.EFContext.DbSet.OrderBy(n => n.Id).FirstOrDefault()
             );
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
                return Common.GetDbServerName(scope.DbConnection, QPContext.CurrentCustomerCode);
            }
        }

        public static IEnumerable<AppSettingsItem> GetAppSettings()
        {
            return QPContext.EFContext.AppSettingsSet.Select(n => new AppSettingsItem { Key = n.Key, Value = n.Value }).ToList();
        }

        public static T GetAppSettings<T>(string name, bool throwExceptions = false)
        {
            AppSettingsDAL setting = QPContext.EFContext.AppSettingsSet
                .FirstOrDefault(x => x.Key == name);

            if (setting is null || string.IsNullOrWhiteSpace(setting.Value))
            {
                if (throwExceptions)
                {
                    throw new InvalidOperationException($"Unable to find setting {name} in QP settings.");
                }
                else
                {
                    return default;
                }
            }

            TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));
            return (T)converter.ConvertFromString(null, CultureInfo.InvariantCulture, setting.Value);
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
