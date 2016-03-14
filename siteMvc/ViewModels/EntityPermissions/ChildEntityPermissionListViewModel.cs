using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.Resources;
using Quantumart.QP8.BLL.Services.EntityPermissions;
using C = Quantumart.QP8.Constants;
using B = Quantumart.QP8.BLL;
using Quantumart.QP8.BLL;
using Quantumart.QP8.WebMvc.ViewModels.Interfaces;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.BLL.Services.DTO;

namespace Quantumart.QP8.WebMvc.ViewModels.EntityPermissions
{
	/// <summary>
	/// Модель списка permission
	/// </summary>
	public class ChildEntityPermissionListViewModel : ListViewModel, IUserAndGroupSearchBlockViewModel
	{
		protected IPermissionListViewModelSettings settings;		
		protected string controlerName;		

		public static ChildEntityPermissionListViewModel Create(ChildPermissionInitListResult result, string tabId, int parentId, IPermissionListViewModelSettings settings, string controlerName)
		{
			ChildEntityPermissionListViewModel model = ViewModel.Create<ChildEntityPermissionListViewModel>(tabId, parentId);
			model.settings = settings;			
			model.controlerName = controlerName;
			model.ShowParentPermissionButton = result.IsParentPermissionsListActionAccessable;
			model.TitleFieldName = "Title";
			return model;
		}

		public IEnumerable<ChildEntityPermissionListItem> Data { get; set; }

		public string GettingDataControllerName { get { return controlerName; } }

		public string GettingDataActionName { get { return "_ChildIndex"; } }
						

		public override string EntityTypeCode
		{
			get { return settings.EntityTypeCode; }
		}

		public override string ActionCode
		{
			get { return settings.ActionCode; }
		}

		public override string ContextMenuCode
		{
			get { return settings.ContextMenuCode; }
		}

		public override string ActionCodeForLink
		{
			get { return settings.ActionCodeForLink; }
		}

		public bool CanHide
		{
			get { return settings.CanHide;  }
		}

		public bool IsPropagateable
		{
			get { return settings.IsPropagateable; }
		}

		public override bool IsListDynamic { get { return true; } }		

		public string ParentPermissionsListAction { get { return settings.ParentPermissionsListAction; } }
		
		public override bool LinkOpenNewTab { get { return true; } }


		public int MemberType { get { return EntityPermission.GROUP_MEMBER_TYPE; } }

		public IEnumerable<ListItem> GetMemberTypes()
		{
			return new[]
			{
				new ListItem(EntityPermission.GROUP_MEMBER_TYPE, EntityPermissionStrings.Group, "GroupMemberPanel"),
				new ListItem(EntityPermission.USER_MEMBER_TYPE, EntityPermissionStrings.User, "UserMemberPanel")				
			};
		}

		public string SearchBlockId { get { return UniqueId("SearchBlockId"); } }

		public bool ShowParentPermissionButton { get; private set; }
		
	}
}