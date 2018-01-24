using System.Collections.Generic;
using System.Linq;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Validators;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using Quantumart.QP8.WebMvc.ViewModels.Abstract;

namespace Quantumart.QP8.WebMvc.ViewModels.PageTemplate
{
    public sealed class SearchInObjectsViewModel : ListViewModel
    {
        public static SearchInObjectsViewModel Create(string tabId, int parentId, int siteId, IPageTemplateService service)
        {
            var model = Create<SearchInObjectsViewModel>(tabId, parentId);
            model._service = service;
            model.SiteId = siteId;
            return model;
        }

        private IPageTemplateService _service;

        public int SiteId { get; set; }

        public override string EntityTypeCode => Constants.EntityTypeCode.TemplateObject;

        public override string ActionCode => Constants.ActionCode.SearchInObjects;

        public string GridElementId => UniqueId("Grid");

        public string FilterElementId => UniqueId("Filter");

        [LocalizedDisplayName("SelectPage", NameResourceType = typeof(TemplateStrings))]
        public int? PageId { get; set; }

        [LocalizedDisplayName("SelectTemplate", NameResourceType = typeof(TemplateStrings))]
        public int? TemplateId { get; set; }

        public QPSelectListItem PageListItem => PageId == null ? null : new QPSelectListItem { Value = PageId.ToString(), Text = _service.ReadPageProperties(PageId.Value).Name, Selected = true };

        private List<ListItem> _templates;

        public List<ListItem> Templates
        {
            get
            {
                if (_templates == null)
                {
                    _templates = _service.GetAllSiteTemplates(SiteId).Select(x => new ListItem { Value = x.Id.ToString(), Text = x.Name }).ToList();
                    _templates.Insert(0, new ListItem { Value = null, Text = TemplateStrings.SelectTemplate, Selected = true });
                }

                return _templates;
            }
            set => _templates = value;
        }

        /// <summary>
        /// костыль для повторного использования метода select pages
        /// </summary>
        public int SiteAnyTemplateId => _service.GetAllSiteTemplates(SiteId).First().Id;

        public override string ContextMenuCode => string.Empty;

        public override bool LinkOpenNewTab => true;

        public override string ActionCodeForLink => Constants.ActionCode.TemplateObjectProperties;
    }
}
