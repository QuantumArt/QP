using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using B = Quantumart.QP8.BLL;
using C = Quantumart.QP8.Constants;

namespace Quantumart.QP8.WebMvc.ViewModels.PageTemplate
{
    public class PageViewModel : LockableEntityViewModel
    {
        private IPageService _service;

        private int templateId;

        internal static PageViewModel Create(BLL.Page page, string tabId, int parentId, IPageService _pageService)
        {
            var model = EntityViewModel.Create<PageViewModel>(page, tabId, parentId);
            model._service = _pageService;
            model.templateId = parentId;
            return model;
        }

        public override string EntityTypeCode
        {
            get { return C.EntityTypeCode.Page; }
        }

        public override string ActionCode
        {
            get { return IsNew ? C.ActionCode.AddNewPage : C.ActionCode.PageProperties; }
        }

		public override string CaptureLockActionCode
		{
			get
			{
				return C.ActionCode.CaptureLockPage;
			}
		}

        public new B.Page Data
        {
            get
            {
                return (B.Page)EntityData;
            }
            set
            {
                EntityData = value;
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
    }
}