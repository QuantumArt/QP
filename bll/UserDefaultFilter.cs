using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Repository.ArticleRepositories;
using Quantumart.QP8.BLL.Repository.ContentRepositories;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Utils.Binders;

namespace Quantumart.QP8.BLL
{
    public class UserDefaultFilter
    {
        public UserDefaultFilter()
        {
            ArticleIDs = Enumerable.Empty<int>().ToList();
        }

        public int UserId { get; set; }

        [Display(Name = "DefaultFilterSite", ResourceType = typeof(UserStrings))]
        public int? SiteId => ContentId.HasValue ? GetContent().SiteId : GetAllSites().FirstOrDefault()?.Id;

        [Display(Name = "DefaultFilterContent", ResourceType = typeof(UserStrings))]
        public int? ContentId { get; set; }

        public Content GetContent() => ContentId.HasValue ? ContentRepository.GetById(ContentId.Value) : null;

        [Display(Name = "DefaultFilterArticles", ResourceType = typeof(UserStrings))]
        [ModelBinder(BinderType = typeof(IdArrayBinder))]
        public IList<int> ArticleIDs { get; set; }

        public IEnumerable<Article> GetArticles() => ArticleRepository.GetList(ArticleIDs);

        private readonly Lazy<IEnumerable<Site>> _allSites = new Lazy<IEnumerable<Site>>(SiteRepository.GetAll);

        public IEnumerable<Site> GetAllSites() => _allSites.Value;
    }
}
