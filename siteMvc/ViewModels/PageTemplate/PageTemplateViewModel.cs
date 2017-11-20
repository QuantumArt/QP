using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Services;

namespace Quantumart.QP8.WebMvc.ViewModels.PageTemplate
{
    public class PageTemplateViewModel : LockableEntityViewModel
    {
        private IPageTemplateService _service;

        public string AggregationListItemsDataAdditionalNamespaceItems { get; set; }

        internal static PageTemplateViewModel Create(BLL.PageTemplate template, string tabId, int parentId, IPageTemplateService pageTemplateService)
        {
            var model = Create<PageTemplateViewModel>(template, tabId, parentId);
            model._service = pageTemplateService;
            return model;
        }

        public override string EntityTypeCode => Constants.EntityTypeCode.PageTemplate;

        public override string ActionCode => IsNew ? Constants.ActionCode.AddNewPageTemplate : Constants.ActionCode.PageTemplateProperties;

        public new BLL.PageTemplate Data
        {
            get => (BLL.PageTemplate)EntityData;
            set => EntityData = value;
        }

        private List<ListItem> _languages;

        public List<ListItem> Languages => _languages ?? (_languages = _service.GetNetLanguagesAsListItems().ToList());

        private List<ListItem> _charsets;

        public List<ListItem> Charsets => _charsets ?? (_charsets = _service.GetCharsetsAsListItems().ToList());

        private List<ListItem> _locales;

        public List<ListItem> Locales => _locales ?? (_locales = _service.GetLocalesAsListItems().ToList());

        internal void DoCustomBinding()
        {
            if (Data.SiteIsDotNet && string.IsNullOrWhiteSpace(Data.NetTemplateName))
            {
                Data.GenerateNetName();
            }

            if (AggregationListItemsDataAdditionalNamespaceItems != null)
            {
                Data.AdditionalNamespaceItems = JsonConvert.DeserializeObject<List<AdditionalNamespace>>(AggregationListItemsDataAdditionalNamespaceItems);
            }

            Data.SetUsings();
        }

        public override string CaptureLockActionCode => Constants.ActionCode.CaptureLockTemplate;
    }
}
