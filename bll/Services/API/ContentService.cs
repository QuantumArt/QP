using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL;

namespace Quantumart.QP8.BLL.Services.API
{
	public class ContentService : ServiceBase 
	{
		public ContentService(string connectionString, int userId) : base(connectionString, userId)
		{

		}

        public ContentService(int userId) : base(userId)
        {

        }

        public IEnumerable<Content> List(int siteId)
		{
			using (new QPConnectionScope(ConnectionString))
			{
				return ContentRepository.GetListBySiteId(siteId);
			}
		}

		public Content Read(int id)
		{
			using (new QPConnectionScope(ConnectionString))
			{
				return ContentRepository.GetById(id);
			}
		}

		public Content Save(Content content)
		{
			using (new QPConnectionScope(ConnectionString))
			{
				QPContext.CurrentUserId = TestedUserId;
				Content result = content.Persist();
				QPContext.CurrentUserId = 0;
				return result;
			}
		}

		public void Delete(int contentId)
		{
			using (new QPConnectionScope(ConnectionString))
			{
				Content content = ContentRepository.GetById(contentId);
				if (content != null)
				{
					QPContext.CurrentUserId = TestedUserId;
					content.Die();
					QPContext.CurrentUserId = 0;
				}
					
			}
		}
	}
}
