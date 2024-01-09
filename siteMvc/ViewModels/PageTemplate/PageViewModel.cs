using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.WebMvc.ViewModels.Abstract;
namespace Quantumart.QP8.WebMvc.ViewModels.PageTemplate
{
    public class PageViewModel : LockableEntityViewModel
    {

        internal static PageViewModel Create(Page page, string tabId, int parentId, IPageService pageService)
        {
            var model = Create<PageViewModel>(page, tabId, parentId);
            model.Charsets = pageService.GetCharsetsAsListItems().ToList();
            model.Locales = pageService.GetLocalesAsListItems().ToList();
            return model;
        }

        public override string EntityTypeCode => Constants.EntityTypeCode.Page;

        public override string ActionCode => IsNew ? Constants.ActionCode.AddNewPage : Constants.ActionCode.PageProperties;

        public override string CaptureLockActionCode => Constants.ActionCode.CaptureLockPage;

        [Required]
        public Page Data
        {
            get => (Page)EntityData;
            set => EntityData = value;
        }

        public List<ListItem> Charsets { get; set; }

        public List<ListItem> Locales { get; set; }
    }
}
