using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Quantumart.QP8.WebMvc.ViewModels.PageTemplate
{
	public sealed class SearchInParametersViewModel : AreaViewModel
	{
		#region creation

		public static SearchInParametersViewModel Create(string tabId, int parentId)
		{
			SearchInParametersViewModel model = ViewModel.Create<SearchInParametersViewModel>(tabId, parentId);
			return model;
		}

		#endregion

		public override string EntityTypeCode
		{
			get { return Constants.EntityTypeCode.Site; }
		}

		public override string ActionCode
		{
			get { return Constants.ActionCode.SearchInObjects; }
		}


		public string GridElementId
		{
			get
			{
				return UniqueId("Grid");
			}
		}

		public string FilterElementId { get { return UniqueId("Filter"); } }
	}
}