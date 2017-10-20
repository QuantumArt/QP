using System;
using System.Collections.Generic;
using System.Linq;
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
            ArticleIDs = Enumerable.Empty<int>().ToList();
        }

        public int UserId { get; set; }

        [LocalizedDisplayName("DefaultFilterSite", NameResourceType = typeof(UserStrings))]
        public int? SiteId => ContentId.HasValue ? GetContent().SiteId : GetAllSites().FirstOrDefault()?.Id;

        [LocalizedDisplayName("DefaultFilterContent", NameResourceType = typeof(UserStrings))]
        public int? ContentId { get; set; }

        public Content GetContent() => ContentId.HasValue ? ContentRepository.GetById(ContentId.Value) : null;

        [LocalizedDisplayName("DefaultFilterArticles", NameResourceType = typeof(UserStrings))]
        public IList<int> ArticleIDs { get; set; }

        public IEnumerable<Article> GetArticles() => ArticleRepository.GetList(ArticleIDs);

        private readonly Lazy<IEnumerable<Site>> _allSites = new Lazy<IEnumerable<Site>>(SiteRepository.GetAll);

        public IEnumerable<Site> GetAllSites() => _allSites.Value;
    }
}
