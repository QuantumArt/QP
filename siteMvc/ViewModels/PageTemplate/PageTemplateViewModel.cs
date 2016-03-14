using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Validators;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using B = Quantumart.QP8.BLL;
using C = Quantumart.QP8.Constants;

namespace Quantumart.QP8.WebMvc.ViewModels.PageTemplate
{    
    public class PageTemplateViewModel : LockableEntityViewModel
    {
        private IPageTemplateService _service;

		public string AggregationListItems_Data_AdditionalNamespaceItems { get; set; }

        internal static PageTemplateViewModel Create(BLL.PageTemplate template, string tabId, int parentId, IPageTemplateService _pageTemplateService)
        {            
            var model = EntityViewModel.Create<PageTemplateViewModel>(template, tabId, parentId);
            model._service = _pageTemplateService;
            model.siteId = parentId;
            return model;
        }

        private int siteId;

        public override string EntityTypeCode
        {
            get { return C.EntityTypeCode.PageTemplate; }
        }

        public override string ActionCode
        {
            get { return IsNew ? C.ActionCode.AddNewPageTemplate : C.ActionCode.PageTemplateProperties; }
        }

        public new B.PageTemplate Data
        {
            get
            {
                return (B.PageTemplate)EntityData;
            }
            set
            {
                EntityData = value;
            }
        }

        private List<B.ListItem> _languages;
        public List<B.ListItem> Languages
        {
            get
            {
                if (_languages == null)
                {
                    _languages = _service.GetNetLanguagesAsListItems().ToList();                    
                }
                return _languages;
            }
        }

        private List<B.ListItem> _charsets;
        public List<B.ListItem> Charsets
        {
            get
            {
                if (_charsets == null)
                {
                    _charsets = _service.GetCharsetsAsListItems().ToList();
                }
                return _charsets;
            }
        }

        private List<B.ListItem> _locales;
        public List<B.ListItem> Locales
        {
            get
            {
                if (_locales == null)
                {
                    _locales = _service.GetLocalesAsListItems().ToList();
                }
                return _locales;
            }
        }        

        public override void Validate(ModelStateDictionary modelState)
        {			
            base.Validate(modelState);
        }

		internal void DoCustomBinding()
		{
			if (Data.SiteIsDotNet && string.IsNullOrWhiteSpace(Data.NetTemplateName))
				Data.GenerateNetName();
			if (AggregationListItems_Data_AdditionalNamespaceItems != null)
			{
				Data.AdditionalNamespaceItems = new JavaScriptSerializer().Deserialize<List<AdditionalNamespace>>(AggregationListItems_Data_AdditionalNamespaceItems);
			}
			Data.SetUsings();
		}

		public override string CaptureLockActionCode
		{
			get
			{
				return C.ActionCode.CaptureLockTemplate;
			}
		}
    }
}