using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Quantumart.QP8.BLL.Repository.ArticleRepositories.SearchParsers;

namespace Quantumart.QP8.BLL.Services.DTO
{
    public class ChildListQuery
    {
        [BindProperty(Name="entityTypeCode")]
        public string EntityTypeCode { get; set; }

        [BindProperty(Name="parentEntityId")]
        public int ParentEntityId { get; set; }

        [BindProperty(Name="entityId")]
        public int? EntityId { get; set; }

        [BindProperty(Name="returnSelf")]
        public bool ReturnSelf { get; set; }

        [BindProperty(Name="filter")]
        public string Filter { get; set; }

        [BindProperty(Name="hostFilter")]
        public string HostFilter { get; set; }

        [BindProperty(Name="selectItemIDs")]
        public string SelectedIdsStr { get; set; }

        [BindProperty(Name="searchQuery")]
        public string SearchQuery { get; set; }

        public IList<ArticleSearchQueryParam> SearchQueryParams => JsonConvert.DeserializeObject<IList<ArticleSearchQueryParam>>(SearchQuery);

        public IList<ArticleContextQueryParam> ContextQueryParams => new ArticleContextQueryParam[] {};

        public ArticleFullTextSearchQueryParser Parser { get; set; }
    }
}
