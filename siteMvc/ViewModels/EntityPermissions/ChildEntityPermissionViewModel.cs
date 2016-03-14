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
using Quantumart.QP8.BLL.Services.DTO;

namespace Quantumart.QP8.WebMvc.ViewModels.EntityPermissions
{
	public class ChildEntityPermissionViewModel : EntityViewModel
	{

		private IChildEntityPermissionService service;
		private IPermissionViewModelSettings settings;
		private string actionCode;		


		public static ChildEntityPermissionViewModel Create(ChildEntityPermission permission, string tabId, int parentId,
			string actionCode, string controllerName, string saveActionName, IChildEntityPermissionService service, int? userId = null, int? groupId = null, IEnumerable<int> IDs = null, bool isPostBack = false)
		{
			ChildEntityPermissionViewModel model = ChildEntityPermissionViewModel.Create<ChildEntityPermissionViewModel>(permission, tabId, parentId);
			model.service = service;
			model.settings = service != null ? service.ViewModelSettings : null;
			model.actionCode = actionCode;
			model.ControllerName = controllerName;
			model.SaveActionName = saveActionName;
			model.EntityIDs = IDs ?? new int[0];
			model.IsPostBack = isPostBack;

			return model;
		}

		public new ChildEntityPermission Data
		{
			get
			{
				return (ChildEntityPermission)EntityData;
			}
			set
			{
				EntityData = value;
			}
		}

		#region override
		public override string EntityTypeCode
		{
			get { return settings.EntityTypeCode; }
		}

		public override string ActionCode
		{
			get { return actionCode; }
		} 
		#endregion

		#region props
		public bool IsPropagateable { get { return settings.IsPropagateable; } }

		public bool CanHide { get { return settings.CanHide; } }
		public IEnumerable<int> EntityIDs { get; set; }
		public string ControllerName { get; private set; }
		public bool IsContentPermission { get { return StringComparer.InvariantCultureIgnoreCase.Equals(EntityTypeCode, Constants.EntityTypeCode.ContentPermission); } }
		#endregion

		#region list items		
		public IEnumerable<ListItem> GetPermissionLevels()
		{
			return service.GetPermissionLevels()
				.Select(l => new ListItem { Value = l.Id.ToString(), Text = Translator.Translate(l.Name) })
				.ToArray();

		}
		#endregion								
	
		public string SaveActionName { get; set; }
		public bool IsPostBack { get; set; }
	}
}