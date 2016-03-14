using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.WebMvc.Extensions.Helpers;

namespace Quantumart.QP8.WebMvc.ViewModels
{
	public abstract class UserViewModelBase : EntityViewModel
	{
		protected IUserService service;

		internal virtual void DoCustomBinding()
		{

		}

		public new User Data
		{
			get
			{
				return (User)EntityData;
			}
			set
			{
				EntityData = value;
			}
		}

		public UserDefaultFilter ContentDefaultFilter
		{
			get
			{
				return Data.ContentDefaultFilters.FirstOrDefault() ?? new UserDefaultFilter { UserId = QPContext.CurrentUserId };
			}
			set
			{
				Data.ContentDefaultFilters = new[] { value };
			}
		}

		public QPSelectListItem SelectedDefaultFilterContentListItem
		{ 
			get 
			{
				BLL.Content content = ContentDefaultFilter.GetContent();
				if (content != null)
					return new QPSelectListItem
					{
						Selected = true,
						Text = content.Name,
						Value = content.Id.ToString()
					};
				else
					return null;
			} 
		}

		public IEnumerable<ListItem> SelectedDefaultFilterArticleListItems 
		{ 
			get 
			{
				return ContentDefaultFilter.GetArticles().Select(a => new ListItem(a.Id.ToString(), a.Name));					
			} 
		}

		public IEnumerable<ListItem> AllSitesListItems 
		{
			get
			{
				return ContentDefaultFilter.GetAllSites().Select(a => new ListItem(a.Id.ToString(), a.Name));	
			}
		}

		public string ContentFilterElementId { get { return UniqueId("contentFilterElementId"); } }

	}


}
