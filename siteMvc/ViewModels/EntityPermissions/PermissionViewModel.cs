using System;
using System.Collections.Generic;
using System.Linq;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services.EntityPermissions;
using Quantumart.QP8.Resources;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using Quantumart.QP8.WebMvc.ViewModels.Abstract;

namespace Quantumart.QP8.WebMvc.ViewModels.EntityPermissions
{
    public class PermissionViewModel : EntityViewModel
    {
        private IPermissionService _service;
        private IPermissionViewModelSettings _settings;

        public static PermissionViewModel Create(EntityPermission permission, string tabId, int parentId, IPermissionService service, IPermissionViewModelSettings settings = null, bool? isPostBack = null)
        {
            var model = Create<PermissionViewModel>(permission, tabId, parentId);
            model._service = service;
            model._settings = settings ?? service.ViewModelSettings;
            model.IsPostBack = isPostBack ?? false;
            model.Init();
            return model;
        }

        public new EntityPermission Data
        {
            get => (EntityPermission)EntityData;
            set => EntityData = value;
        }

        private void Init()
        {
            if (IsNew)
            {
                Data.ParentEntityId = ParentEntityId;
            }

            Data.Init(_service.Repository);
        }


        public override string EntityTypeCode => _settings.EntityTypeCode;

        public override string ActionCode => _settings.ActionCode;

        public bool IsPropagateable => _settings.IsPropagateable;

        public bool CanHide => _settings.CanHide;

        public QPSelectListItem UserListItem => Data.User != null ? new QPSelectListItem { Value = Data.User.Id.ToString(), Text = Data.User.LogOn, Selected = true } : null;

        public QPSelectListItem GroupListItem => Data.Group != null ? new QPSelectListItem { Value = Data.Group.Id.ToString(), Text = Data.Group.Name, Selected = true } : null;

        public bool IsPostBack { get; set; }

        public bool IsContentPermission => StringComparer.InvariantCultureIgnoreCase.Equals(EntityTypeCode, Constants.EntityTypeCode.ContentPermission);

        public IEnumerable<ListItem> GetMemberTypes() => new[]
        {
            new ListItem(EntityPermission.GroupMemberType, EntityPermissionStrings.Group, "GroupMemberPanel"),
            new ListItem(EntityPermission.UserMemberType, EntityPermissionStrings.User, "UserMemberPanel")
        };

        public IEnumerable<ListItem> GetPermissionLevels()
        {
            return _service.GetPermissionLevels().Select(l => new ListItem { Value = l.Id.ToString(), Text = Translator.Translate(l.Name) }).ToArray();
        }
    }
}
