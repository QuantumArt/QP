using System.Collections.Generic;
using System.Linq;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Validators;
using Quantumart.QP8.WebMvc.ViewModels.Abstract;

namespace Quantumart.QP8.WebMvc.ViewModels
{
    public class UserGroupViewModel : EntityViewModel
    {
        private IUserGroupService _service;

        public new UserGroup Data
        {
            get
            {
                return (UserGroup)EntityData;
            }
            set
            {
                EntityData = value;
            }
        }

        public static UserGroupViewModel Create(UserGroup group, string tabId, int parentId, IUserGroupService service)
        {
            var model = Create<UserGroupViewModel>(group, tabId, parentId);
            model._service = service;
            model.Init();
            return model;
        }

        public override string EntityTypeCode => Constants.EntityTypeCode.UserGroup;

        public override string ActionCode => IsNew ? Constants.ActionCode.AddNewUserGroup : Constants.ActionCode.UserGroupProperties;

        private void Init()
        {
            if (IsNew)
            {
                Data.Users = Enumerable.Empty<User>();
            }

            BindedUserIDs = Data.Users.Select(u => u.Id).ToArray();
            ParentGroupId = Data.ParentGroup?.Id ?? 0;
        }

        internal void DoCustomBinding()
        {
            Data.Users = BindedUserIDs.Any() ? _service.GetUsers(BindedUserIDs) : Enumerable.Empty<User>();
            Data.ParentGroup = ParentGroupId.HasValue ? _service.Read(ParentGroupId.Value) : null;
        }

        [LocalizedDisplayName("BindedUserIDs", NameResourceType = typeof(UserGroupStrings))]
        public IEnumerable<int> BindedUserIDs { get; set; }

        [LocalizedDisplayName("ParentGroup", NameResourceType = typeof(UserGroupStrings))]
        public int? ParentGroupId { get; set; }

        public IEnumerable<ListItem> BindedUserListItems
        {
            get
            {
                return Data.Users
                    .Select(u => new ListItem(u.Id.ToString(), u.Name))
                    .ToArray();
            }
        }

        public IEnumerable<ListItem> GetGroupList()
        {
            return new[]
            {
                new ListItem(string.Empty, UserGroupStrings.SelectParentGroup)
            }
            .Concat(_service.GetAllGroups()
            .Where(g => g.Id != Data.Id && !g.UseParallelWorkflow)
            .Select(g => new ListItem(g.Id.ToString(), g.Name)))
            .ToArray();
        }
    }
}
