using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Quantumart.QP8.BLL;
using Quantumart.QP8.Resources;
using C = Quantumart.QP8.Constants;
using Quantumart.QP8.BLL.Services;


namespace Quantumart.QP8.WebMvc.ViewModels
{
    public class ArticleVersionListViewModel : ListViewModel
    {
		#region creation

		public static ArticleVersionListViewModel Create(string tabId, int parentId)
		{
			ArticleVersionListViewModel model = ViewModel.Create<ArticleVersionListViewModel>(tabId, parentId);
			model.ShowAddNewItemButton = !model.IsWindow;
			return model;
		}

		#endregion

		#region read-only members

		public override string EntityTypeCode
		{
			get
			{
				return C.EntityTypeCode.ArticleVersion;
			}
		}

		public override string ActionCode
		{
			get
			{
				return C.ActionCode.ArticleVersions;
			}
		}		

		#endregion
	}
}