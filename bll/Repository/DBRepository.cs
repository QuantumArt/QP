using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Quantumart.QP8.BLL.Mappers;
using Quantumart.QP8.DAL;
using Quantumart.QP8.BLL.Helpers;

namespace Quantumart.QP8.BLL.Repository
{
	public class DbRepository
	{
		
		public static Db Get()
		{
			var result = MappersRepository.DbMapper.GetBizObject(
				QPContext.EFContext.DbSet.Include("LastModifiedByUser").FirstOrDefault()
			);
			result.AppSettings = GetAppSettings();
			return result;
		}

		public static Db GetForUpdate()
		{
			var result = MappersRepository.DbMapper.GetBizObject(
				QPContext.EFContext.DbSet.FirstOrDefault()
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
		
		public static bool RecordActions
		{
			get
			{
				return Get().RecordActions;
			}
		}

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
			IEnumerable<AppSettingsDAL> result = values.Select(n => AppSettingsDAL.CreateAppSettingsDAL(n.Key, n.Value)).ToList();
			DefaultRepository.SimpleSave(result);
		}

		internal static void DeleteAppSettings()
		{
			DefaultRepository.SimpleDelete(QPContext.EFContext.AppSettingsSet.ToList());
		}

	}
}
