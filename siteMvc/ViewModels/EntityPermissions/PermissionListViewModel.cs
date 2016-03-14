using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.Resources;
using Quantumart.QP8.BLL.Services.EntityPermissions;
using C = Quantumart.QP8.Constants;
using B = Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.BLL.Services.DTO;
namespace Quantumart.QP8.WebMvc.ViewModels.EntityPermissions
{
	/// <summary>
	/// Модель списка permission
	/// </summary>
	public class PermissionListViewModel : ListViewModel
	{
		protected IPermissionListViewModelSettings settings;		
		protected string controlerName;
		private Lazy<B.Article> parentArticle;
		private bool isEnableArticlesPermissionsAccessable;		

		public static PermissionListViewModel Create(PermissionInitListResult result, string tabId, int parentId, IPermissionService service, string controlerName)
		{
			PermissionListViewModel model = ViewModel.Create<PermissionListViewModel>(tabId, parentId);
			model.settings = service.ListViewModelSettings;			
			model.controlerName = controlerName;			
			model.parentArticle = new Lazy<B.Article>(() => service.GetParentArticle(parentId));
			model.ShowAddNewItemButton = result.IsAddNewAccessable && !model.IsWindow;
			model.isEnableArticlesPermissionsAccessable = result.IsEnableArticlesPermissionsAccessable;
			model.TitleFieldName = "Id";
			return model;			
		}

		public IEnumerable<EntityPermissionListItem> Data { get; set; }

		public string GettingDataControllerName { get { return controlerName; } }

		public string GettingDataActionName { get { return "_Index"; } }

		public bool IsPropagateable { get { return settings.IsPropagateable; } }

		public bool CanHide { get { return settings.CanHide; } }

		public string PermissionEntityTypeCode { get { return settings.PermissionEntityTypeCode; } }
		
		public bool ShowDisableArticlePermissionForContentWarning
		{
			get
			{
				if (PermissionEntityTypeCode == C.EntityTypeCode.ArticlePermission)
				{
					return !ParentArticle.Content.AllowItemsPermission && isEnableArticlesPermissionsAccessable;
				}
				else
					return false;
			}
		}
		
		
		public B.Article ParentArticle
		{
			get
			{
				if (PermissionEntityTypeCode == C.EntityTypeCode.ArticlePermission)
					return parentArticle.Value;
				else
					return null;
			}
		}

		public override string AddNewItemText
		{
			get
			{
				return EntityPermissionStrings.AddNewPermission;
			}
		}
		

		public override string EntityTypeCode
		{
			get { return settings.EntityTypeCode; }
		}

		public override string ActionCode
		{
			get { return settings.ActionCode; }
		}

		public override string AddNewItemActionCode
		{
			get
			{
				return settings.AddNewItemActionCode;
			}
		}

		public override bool LinkOpenNewTab { get { return true; } }		
	}
}