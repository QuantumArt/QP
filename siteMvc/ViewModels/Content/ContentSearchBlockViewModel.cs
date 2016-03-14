using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Extensions.Helpers;

namespace Quantumart.QP8.WebMvc.ViewModels.Content
{
	public class ContentSearchBlockViewModel
	{
		public ContentSearchBlockViewModel(int id, string actionCode, string hostId)
		{
			this.id = id;
			this.actionCode = actionCode;
			this.hostId = hostId;

			_contentGroups = new Lazy<IEnumerable<ListItem>>(() => ContentService.GetSiteContentGroupsForFilter(this.id));
			_sites = new Lazy<IEnumerable<ListItem>>(() => SiteService.GetAllSites());
		}

		private readonly int id;
		private readonly string actionCode;
		private readonly string hostId;
		private readonly Lazy<IEnumerable<ListItem>> _contentGroups;
		private readonly Lazy<IEnumerable<ListItem>> _sites;


		public IEnumerable<ListItem> ContentGroups { get { return _contentGroups.Value; } }

		public IEnumerable<ListItem> Sites { get { return _sites.Value; } }

		public bool ShowSiteList 
		{
			get
			{
				return this.actionCode.Equals(ActionCode.MultipleSelectContent, StringComparison.InvariantCultureIgnoreCase);				
			}
		}

		public bool ShowGroupList { 
			get 
			{ 
				return !ShowSiteList; 
			} 
		}

		public string UniqueId(string id)
		{
			return HtmlHelperFieldExtensions.UniqueId(id, hostId);
		}
	}
}