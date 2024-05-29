using System;
using QP8.Infrastructure;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Services.API
{
    public class ServiceBase
    {
        private bool _userTested;


        protected ServiceBase(int userId)
            : this(QPContext.CurrentDbConnectionInfo, userId)
        {
        }

        protected ServiceBase(string connectionString, int userId) : this(
            new QpConnectionInfo(connectionString, DatabaseType.SqlServer), userId
        )
        {

            ConnectionString = connectionString;
            UserId = userId;
        }

        protected ServiceBase(QpConnectionInfo info, int userId)
        {
            Ensure.NotNull(info, "Connection info should not be null");
            Ensure.NotNull(info.ConnectionString, "Connection string should not be empty");
            ConnectionString = info.ConnectionString;
            DbType = info.DbType;
            UserId = userId;
        }


        public string ConnectionString { get; }

        public DatabaseType DbType { get; }

        public QpConnectionInfo ConnectionInfo => new QpConnectionInfo(ConnectionString, DbType);

        public int UserId { get; }

        public int TestedUserId
        {
            get
            {
                if (!_userTested)
                {
                    TestUser();
                }

                return UserId;
            }
        }

        public bool IsLive
        {
            get => QPContext.IsLive;
            set => QPContext.IsLive = value;
        }

        public void LoadStructureCache()
        {
            LoadStructureCache(null, false);
        }

        public void LoadStructureCache(IContextStorage st)
        {
            LoadStructureCache(st, false);
        }

        public void LoadStructureCache(IContextStorage st, bool resetExternal)
        {
            using (new QPConnectionScope(ConnectionInfo))
            {
                if (st != null)
                {
                    QPContext.ExternalContextStorage = st;
                }

                QPContext.LoadStructureCache(resetExternal);
            }
        }

        private void TestUser()
        {
            using (new QPConnectionScope(ConnectionInfo))
            {
                var user = UserRepository.GetById(UserId);
                if (user == null)
                {
                    throw new ApplicationException(string.Format(UserStrings.UserNotFound, UserId));
                }

                _userTested = true;
            }
        }
    }
}
