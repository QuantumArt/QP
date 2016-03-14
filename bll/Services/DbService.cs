using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.BLL.Repository.Articles;
using System.Security.Cryptography;

namespace Quantumart.QP8.BLL.Services
{
	public class DbService
	{
		public static Db ReadSettings()
		{
			return DbRepository.Get();
		}

		public static Db ReadSettingsForUpdate()
		{
			return DbRepository.GetForUpdate();
		}

		public static Db UpdateSettings(Db db)
		{
			Db result = DbRepository.Update(db);
			return result;
		}

		public static string GetDbHash()
		{
			return GehHash(GehHash(DbRepository.GetDbServerName()) + GehHash(DbRepository.GetDbName()));
		}

		private static string GehHash(string value)
		{
			byte[] data = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(value));
			StringBuilder sBuilder = new StringBuilder();

			for (int i = 0; i < data.Length; i++)
			{
				sBuilder.Append(data[i].ToString("x2"));
			}

			return sBuilder.ToString();
		}

		public static HomeResult Home()
		{
			return new HomeResult()
			{
				Sites = SiteService.GetSites(),
				CurrentUser = new UserService().ReadProperties(QPContext.CurrentUserId),
				LockedCount = ArticleRepository.GetLockedCount(),
				ApprovalCount = ArticleRepository.GetForApprovalCount()
			};
		}
	}
}
