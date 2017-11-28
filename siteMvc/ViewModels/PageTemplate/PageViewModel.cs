using System.Collections.Generic;
using System.Linq;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.WebMvc.ViewModels.Abstract;

namespace Quantumart.QP8.WebMvc.ViewModels.PageTemplate
{
    public class PageViewModel : LockableEntityViewModel
    {
        private IPageService _service;

        internal static PageViewModel Create(Page page, string tabId, int parentId, IPageService pageService)
        {
            var model = Create<PageViewModel>(page, tabId, parentId);
            model._service = pageService;
            return model;
        }

        public override string EntityTypeCode => Constants.EntityTypeCode.Page;

        public override string ActionCode => IsNew ? Constants.ActionCode.AddNewPage : Constants.ActionCode.PageProperties;

        public override string CaptureLockActionCode => Constants.ActionCode.CaptureLockPage;

        public new Page Data
        {
            get => (Page)EntityData;
            set => EntityData = value;
        }

        private List<ListItem> _charsets;

        public List<ListItem> Charsets => _charsets ?? (_charsets = _service.GetCharsetsAsListItems().ToList());

        private List<ListItem> _locales;

        public List<ListItem> Locales => _locales ?? (_locales = _service.GetLocalesAsListItems().ToList());
    }
}
