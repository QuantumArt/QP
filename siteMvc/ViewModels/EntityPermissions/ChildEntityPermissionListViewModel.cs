using System.Collections.Generic;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.BLL.Services.EntityPermissions;
using Quantumart.QP8.Resources;
using Quantumart.QP8.WebMvc.ViewModels.Interfaces;

namespace Quantumart.QP8.WebMvc.ViewModels.EntityPermissions
{
    public class ChildEntityPermissionListViewModel : ListViewModel, IUserAndGroupSearchBlockViewModel
    {
        protected IPermissionListViewModelSettings Settings;
        protected string ControlerName;

        public static ChildEntityPermissionListViewModel Create(ChildPermissionInitListResult result, string tabId, int parentId, IPermissionListViewModelSettings settings, string controlerName)
        {
            var model = Create<ChildEntityPermissionListViewModel>(tabId, parentId);
            model.Settings = settings;
            model.ControlerName = controlerName;
            model.ShowParentPermissionButton = result.IsParentPermissionsListActionAccessable;
            model.TitleFieldName = "Title";
            return model;
        }

        public IEnumerable<ChildEntityPermissionListItem> Data { get; set; }

        public string GettingDataControllerName => ControlerName;

        public string GettingDataActionName => "_ChildIndex";

        public override string EntityTypeCode => Settings.EntityTypeCode;

        public override string ActionCode => Settings.ActionCode;

        public override string ContextMenuCode => Settings.ContextMenuCode;

        public override string ActionCodeForLink => Settings.ActionCodeForLink;

        public bool CanHide => Settings.CanHide;

        public bool IsPropagateable => Settings.IsPropagateable;

        public override bool IsListDynamic => true;

        public string ParentPermissionsListAction => Settings.ParentPermissionsListAction;

        public override bool LinkOpenNewTab => true;

		public int MemberType { get { return EntityPermission.GroupMemberType; } }

        public IEnumerable<ListItem> GetMemberTypes()
        {
            return new[]
            {
				new ListItem(EntityPermission.GroupMemberType, EntityPermissionStrings.Group, "GroupMemberPanel"),
				new ListItem(EntityPermission.UserMemberType, EntityPermissionStrings.User, "UserMemberPanel")
            };
        }

        public string SearchBlockId => UniqueId("SearchBlockId");

        public bool ShowParentPermissionButton { get; private set; }

    }
}
