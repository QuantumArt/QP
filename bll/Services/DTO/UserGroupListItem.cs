using System;

namespace Quantumart.QP8.BLL.Services.DTO
{
	public class UserGroupListItem
	{
		public int Id { get; set; }

		public string Name { get; set; }

		public string Description { get; set; }

		public bool SharedArticles { get; set; }

		public DateTime Created { get; set; }

		public DateTime Modified { get; set; }

		public int LastModifiedByUserId { get; set; }

		public string LastModifiedByUser { get; set; }


		public string SharedArticlesChecked => SharedArticles ? "checked=\"checked\"" : null;
	}
}
