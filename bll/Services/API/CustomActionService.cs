using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Configuration;

namespace Quantumart.QP8.BLL.Services.API
{
    public class CustomActionService : ServiceBase
    {
        public CustomActionService(QpConnectionInfo info, int userId)
            : base(info, userId)
        {
        }
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
            using (new QPConnectionScope(ConnectionInfo))
            {
                return CustomActionRepository.GetByCode(code);
            }
        }
    }
}
