using System.Collections.Generic;
using System.Linq;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using Quantumart.QP8.WebMvc.ViewModels.Abstract;

namespace Quantumart.QP8.WebMvc.ViewModels.User
{
    public abstract class UserViewModelBase : EntityViewModel
    {
        protected IUserService Service;

        internal virtual void DoCustomBinding()
        {
        }

        public new BLL.User Data
        {
            get => (BLL.User)EntityData;
            set => EntityData = value;
        }

        public UserDefaultFilter ContentDefaultFilter
        {
            get => Data.ContentDefaultFilters.FirstOrDefault() ?? new UserDefaultFilter { UserId = QPContext.CurrentUserId };
            set => Data.ContentDefaultFilters = new[] { value };
        }

        public QPSelectListItem SelectedDefaultFilterContentListItem
        {
            get
            {
                var content = ContentDefaultFilter.GetContent();
                if (content != null)
                {
                    return new QPSelectListItem
                    {
                        Selected = true,
                        Text = content.Name,
                        Value = content.Id.ToString()
                    };
                }

                return null;
            }
        }

        public IEnumerable<ListItem> SelectedDefaultFilterArticleListItems
        {
            get { return ContentDefaultFilter.GetArticles().Select(a => new ListItem(a.Id.ToString(), a.Name)); }
        }

        public IEnumerable<ListItem> AllSitesListItems
        {
            get { return ContentDefaultFilter.GetAllSites().Select(a => new ListItem(a.Id.ToString(), a.Name)); }
        }

        public string ContentFilterElementId => UniqueId("contentFilterElementId");
    }
}
