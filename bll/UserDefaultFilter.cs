using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Repository.Articles;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Validators;

namespace Quantumart.QP8.BLL
{		
	public class UserDefaultFilter
	{
		public UserDefaultFilter()
		{
			ArticleIDs = Enumerable.Empty<int>();
		}

		public int UserId { get; set; }

		[LocalizedDisplayName("DefaultFilterSite", NameResourceType = typeof(UserStrings))]		
		public int? SiteId 
		{ 
			get 
			{
				if (ContentId.HasValue)
					return GetContent().SiteId;
				else
				{
					Site site = GetAllSites().FirstOrDefault();
					if (site != null)
						return site.Id;
					else
						return null;
				}
			} 
		}

		[LocalizedDisplayName("DefaultFilterContent", NameResourceType = typeof(UserStrings))]
		public int? ContentId { get; set; }		
		public Content GetContent()
		{
			if (ContentId.HasValue)
				return ContentRepository.GetById(ContentId.Value);
			else
				return null;
		}		

		[LocalizedDisplayName("DefaultFilterArticles", NameResourceType = typeof(UserStrings))]
		public IEnumerable<int> ArticleIDs { get; set; }
		public IEnumerable<Article> GetArticles()
		{
			return ArticleRepository.GetList(ArticleIDs);
		}

		private Lazy<IEnumerable<Site>> allSites = new Lazy<IEnumerable<Site>>(SiteRepository.GetAll);
		public IEnumerable<Site> GetAllSites()
		{
			return allSites.Value;
		}
	}
}
