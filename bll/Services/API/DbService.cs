﻿using System.Collections.Generic;
using System.Linq;
using Quantumart.QP8.BLL.Repository;

namespace Quantumart.QP8.BLL.Services.API
{
    public class DbService : ServiceBase
    {
        public DbService(string connectionString, int userId)
            : base(connectionString, userId)
        {
        }

        public DbService(int userId)
            : base(userId)
        {
        }

        public Dictionary<string, string> GetAppSettings()
        {
            using (new QPConnectionScope(ConnectionInfo))
            {
                return DbRepository.GetAppSettings().ToDictionary(n => n.Key, n => n.Value);
            }
        }
    }
}
