using System;
using System.Collections.Generic;
using System.Linq;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services.EntityPermissions;
using Quantumart.QP8.WebMvc.ViewModels.Abstract;

namespace Quantumart.QP8.WebMvc.ViewModels.EntityPermissions
{
    public class ChildEntityPermissionViewModel : EntityViewModel
    {
        private IChildEntityPermissionService _service;
        private IPermissionViewModelSettings _settings;
        private string _actionCode;

        public static ChildEntityPermissionViewModel Create(ChildEntityPermission permission, string tabId, int parentId, string actionCode, string controllerName, string saveActionName, IChildEntityPermissionService service, int? userId = null, int? groupId = null, IEnumerable<int> ids = null, bool isPostBack = false)
        {
            var model = Create<ChildEntityPermissionViewModel>(permission, tabId, parentId);
            model._service = service;
            model._settings = service?.ViewModelSettings;
            model._actionCode = actionCode;
            model.ControllerName = controllerName;
            model.SaveActionName = saveActionName;
            model.EntityIDs = ids ?? new int[0];
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

        public override string EntityTypeCode => _settings.EntityTypeCode;

        public override string ActionCode => _actionCode;

        public bool IsPropagateable => _settings.IsPropagateable;

        public bool CanHide => _settings.CanHide;

        public IEnumerable<int> EntityIDs { get; set; }

        public string ControllerName { get; private set; }

        public bool IsContentPermission => StringComparer.InvariantCultureIgnoreCase.Equals(EntityTypeCode, Constants.EntityTypeCode.ContentPermission);

        public IEnumerable<ListItem> GetPermissionLevels()
        {
            return _service.GetPermissionLevels().Select(l => new ListItem { Value = l.Id.ToString(), Text = Translator.Translate(l.Name) }).ToArray();
        }

        public string SaveActionName { get; set; }

        public bool IsPostBack { get; set; }
    }
}
