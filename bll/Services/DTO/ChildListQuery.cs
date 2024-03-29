using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Quantumart.QP8.BLL.Repository.ArticleRepositories.SearchParsers;
using System.Collections.Generic;

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
        public CustomFilterItem[] Filter { get; set; }

        [BindProperty(Name="hostFilter")]
        public CustomFilterItem[] HostFilter { get; set; }

        [BindProperty(Name="selectItemIDs")]
        public string SelectItemIDs { get; set; }

        [BindProperty(Name="searchQuery")]
        public string SearchQuery { get; set; }

        public IList<ArticleSearchQueryParam> SearchQueryParams
        {
            get
            {
                return SearchQuery != null ?
                JsonConvert.DeserializeObject<IList<ArticleSearchQueryParam>>(SearchQuery) :
                new ArticleSearchQueryParam[] { };
            }
        }

        public IList<ArticleContextQueryParam> ContextQueryParams => new ArticleContextQueryParam[] {};

        public ArticleFullTextSearchQueryParser Parser { get; set; }
    }
}
