using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using B = Quantumart.QP8.BLL;
using Quantumart.QP8.Resources;
using C = Quantumart.QP8.Constants;

namespace Quantumart.QP8.WebMvc.ViewModels
{
	public class SearchInArticlesListViewModel : ListViewModel
	{
		public int SiteId { get; set; }

		public string Query { get; set; }

		#region creation

		public static SearchInArticlesListViewModel Create(int id, string tabId, int parentId)
		{
			SearchInArticlesListViewModel model = ViewModel.Create<SearchInArticlesListViewModel>(tabId, parentId);
			model.SiteId = id;
			model.ShowAddNewItemButton = !model.IsWindow;
			model.AutoLoad = false;
			return model;
		}		

		#endregion

		#region read-only members

		public override string EntityTypeCode
		{
			get { return C.EntityTypeCode.Article; }
		}

		public override string ActionCode
		{
			get { return C.ActionCode.SearchInArticles; }
		}

		#endregion		

		public override bool AllowMultipleEntitySelection
		{
			get
			{
				return false;
			}			
		}

		public override string ContextMenuCode
		{
			get
			{
				return String.Empty;
			}
		}

		public override bool LinkOpenNewTab
		{
			get
			{
				return true;
			}
		}

		public string DataBindingControllerName
		{
			get { return "Site"; }
		}

		public string DataBindingActionName
		{
			get { return "_SearchInArticles"; }
		}

		public string SeachBlockElementId
		{
			get { return UniqueId("SearchInArticlesBlock"); }
		}

		public string SearchTextBoxElementId
		{
			get { return UniqueId("SearchInArticlesTextBox"); }
		}

		public string SearchButtonElementId
		{
			get { return UniqueId("SearchInArticlesSearchButton"); }
		}

		public string SearchLayoutFormElementId
		{
			get { return UniqueId("SearchLayoutForm"); }
		}
	}

}