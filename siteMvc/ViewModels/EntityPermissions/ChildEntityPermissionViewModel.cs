using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.BLL.Services.EntityPermissions;
using Quantumart.QP8.WebMvc.ViewModels.Abstract;

namespace Quantumart.QP8.WebMvc.ViewModels.EntityPermissions
{
    public class ChildEntityPermissionViewModel : EntityViewModel
    {
        public static ChildEntityPermissionViewModel Create(ChildEntityPermission permission, string tabId, int parentId, string actionCode, string controllerName, string saveActionName, IChildEntityPermissionService service, int? userId = null, int? groupId = null, IEnumerable<int> ids = null, bool isPostBack = false)
        {
            var model = Create<ChildEntityPermissionViewModel>(permission, tabId, parentId);
            model.CurrentActionCode = actionCode;
            model.IsPropagateable = service.ViewModelSettings.IsPropagateable;
            model.CurrentEntityTypeCode = service.ViewModelSettings.EntityTypeCode;
            model.CanHide = service.ViewModelSettings.CanHide;
            model.PermissionLevels = GetPermissionLevels(service);
            model.ControllerName = controllerName;
            model.SaveActionName = saveActionName;
            model.EntityIds = ids?.ToList() ?? new List<int>();
            model.IsPostBack = isPostBack;
            return model;
        }

        public ChildEntityPermission Data
        {
            get => (ChildEntityPermission)EntityData;
            set => EntityData = value;
        }

        public override string EntityTypeCode => CurrentEntityTypeCode;
        public override string ActionCode => CurrentActionCode;

        public bool IsPropagateable { get; set; }

        public bool CanHide { get; set; }

        public string CurrentEntityTypeCode { get; set; }

        [FromForm(Name="EntityIDs")]
        public List<int> EntityIds { get; set; }

        public string ControllerName { get; set; }

        public bool IsContentPermission => StringComparer.InvariantCultureIgnoreCase.Equals(EntityTypeCode, Constants.EntityTypeCode.ContentPermission);

        public static IEnumerable<ListItem> GetPermissionLevels(IChildEntityPermissionService service)
        {
            return service.GetPermissionLevels().Select(l => new ListItem { Value = l.Id.ToString(), Text = Translator.Translate(l.Name) }).ToArray();
        }

        public IEnumerable<ListItem> PermissionLevels { get; set; }

        public string CurrentActionCode { get; set; }

        public string SaveActionName { get; set; }

        public bool IsPostBack { get; set; }
    }
}
