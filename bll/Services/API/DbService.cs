using Quantumart.QP8.BLL.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quantumart.QP8.BLL.Services.API
{
	public class DbService : ServiceBase
	{

		public DbService(string connectionString, int userId) :  base(connectionString, userId)
		{

		}

        public DbService(int userId) : base(userId)
        {

        }

        public Dictionary<string, string> GetAppSettings()
		{
			using (new QPConnectionScope(ConnectionString))
			{
				return DbRepository.GetAppSettings().ToDictionary(n => n.Key, n => n.Value);
			}
		}
	}
}
