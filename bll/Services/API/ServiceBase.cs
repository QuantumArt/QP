using System;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Services.API
{
	public class ServiceBase
	{
		private string connectionString;
		private int userId;
		private bool userTested = false;

		protected ServiceBase(string connectionString, int userId)
		{
			if (String.IsNullOrEmpty(connectionString))
				throw new ArgumentNullException("connectionString");

			this.ConnectionString = connectionString;
			this.UserId = userId;
		}

		public string ConnectionString
		{
			get { return connectionString; }
			private set { connectionString = value; }
		}

		public int UserId
		{
			get { return userId; }
			private set { userId = value; }
		}

		public int TestedUserId
		{
			get
			{
				if (!userTested)
					TestUser();
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
					QPContext.ExternalContextStorage = st;
				QPContext.LoadStructureCache(resetExternal);
			}
		}

		private void TestUser()
		{
			using (new QPConnectionScope(ConnectionString))
			{
				var user = UserRepository.GetById(UserId);
				if (user == null)
					throw new ApplicationException(String.Format(UserStrings.UserNotFound, UserId));
				userTested = true;
			}
			
		}
	}
}