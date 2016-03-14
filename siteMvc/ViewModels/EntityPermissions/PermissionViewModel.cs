using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services.EntityPermissions;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Validators;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.WebMvc.ViewModels.EntityPermissions
{
	public class PermissionViewModel : EntityViewModel
	{
		

		private IPermissionService service;
		private IPermissionViewModelSettings settings;

		public static PermissionViewModel Create(EntityPermission permission, string tabId, int parentId, IPermissionService service, IPermissionViewModelSettings settings = null, bool? isPostBack = null)
		{
			PermissionViewModel model = EntityViewModel.Create<PermissionViewModel>(permission, tabId, parentId);
			model.service = service;
			model.settings = settings ?? service.ViewModelSettings;
			model.IsPostBack = isPostBack ?? false;
			model.Init();
			return model;
		}
		
		public new EntityPermission Data
		{
			get
			{
				return (EntityPermission)EntityData;
			}
			set
			{
				EntityData = value;
			}
		}

		private void Init()
		{
			if (IsNew)
				Data.ParentEntityId = ParentEntityId;
			Data.Init(service.Repository);
		}

		internal void DoCustomBinding()
		{
			Data.DoCustomBinding();
		}

		#region override
		public override string EntityTypeCode
		{
			get { return settings.EntityTypeCode; }
		}

		public override string ActionCode
		{
			get { return settings.ActionCode; }
		} 
		#endregion

		#region props
		public bool IsPropagateable { get { return settings.IsPropagateable; } }

		public bool CanHide { get { return settings.CanHide; } }

		public QPSelectListItem UserListItem { get { return Data.User != null ? new QPSelectListItem { Value = Data.User.Id.ToString(), Text = Data.User.LogOn, Selected = true } : null; } }

		public QPSelectListItem GroupListItem { get { return Data.Group != null ? new QPSelectListItem { Value = Data.Group.Id.ToString(), Text = Data.Group.Name, Selected = true } : null; } }

		public bool IsPostBack { get; set; }

		public bool IsContentPermission { get { return StringComparer.InvariantCultureIgnoreCase.Equals(EntityTypeCode, Constants.EntityTypeCode.ContentPermission); } }
		#endregion

		#region list items
		public IEnumerable<ListItem> GetMemberTypes()
		{
			return new[]
			{
				new ListItem(EntityPermission.GROUP_MEMBER_TYPE, EntityPermissionStrings.Group, "GroupMemberPanel"),
				new ListItem(EntityPermission.USER_MEMBER_TYPE, EntityPermissionStrings.User, "UserMemberPanel")			
			};
		}

		public IEnumerable<ListItem> GetPermissionLevels()
		{
			return service.GetPermissionLevels()
				.Select(l => new ListItem { Value = l.Id.ToString(), Text = Translator.Translate(l.Name) })
				.ToArray();

		}
		#endregion						
	}
}