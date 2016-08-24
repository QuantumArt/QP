using System;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Services.API
{
    public class ServiceBase
    {
        private bool _userTested;

        protected ServiceBase(string connectionString, int userId)
        {
            Setup(connectionString, userId);
        }
        private void Setup(string connectionString, int userId)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException(nameof(connectionString));

            ConnectionString = connectionString;
            UserId = userId;
        }

        protected ServiceBase(int userId)
        {
            if (QPConnectionScope.Current == null)
                throw new ApplicationException("Attempt to create service instance without external QPConnectionScope object");

            Setup(QPConnectionScope.Current.DbConnection.ConnectionString, userId);
        }

        public string ConnectionString { get; private set; }

        public int UserId { get; private set; }

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
