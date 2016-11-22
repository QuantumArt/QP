using System;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Services.API
{
    public class ServiceBase
    {
        private bool _userTested;

        protected ServiceBase(int userId)
            : this(QPContext.CurrentDbConnectionString, userId)
        {
        }

        protected ServiceBase(string connectionString, int userId)
        {
            Ensure.NotNull(connectionString, "Connection string should not be empty");
            ConnectionString = connectionString;
            UserId = userId;
        }

        public string ConnectionString { get; }

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
            get
            {
                return QPContext.IsLive;
            }
            set
            {
                QPContext.IsLive = value;
            }
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
            using (new QPConnectionScope(ConnectionString))
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
            using (new QPConnectionScope(ConnectionString))
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
