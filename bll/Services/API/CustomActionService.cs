﻿using Quantumart.QP8.BLL.Repository;

namespace Quantumart.QP8.BLL.Services.API
{
    public class CustomActionService : ServiceBase
    {
        public CustomActionService(string connectionString, int userId)
            : base(connectionString, userId)
        {
        }

        public CustomActionService(int userId)
            : base(userId)
        {
        }

        public CustomAction ReadByCode(string code)
        {
            using (new QPConnectionScope(ConnectionString))
            {
                return CustomActionRepository.GetByCode(code);
            }
        }
    }
}
