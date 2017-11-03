using System.Collections.Generic;
using Quantumart.QP8.BLL.Services.DTO;

namespace Quantumart.QP8.BLL.Services
{
    public interface IUserGroupService
    {
        UserGroupInitListResult InitList(int parentId);

        ListResult<UserGroupListItem> List(ListCommand cmd, IEnumerable<int> selectedIds = null);

        IEnumerable<User> GetUsers(IEnumerable<int> userIDs);

        UserGroup ReadProperties(int id);

        UserGroup Read(int id);

        IEnumerable<UserGroup> GetAllGroups();

        UserGroup UpdateProperties(UserGroup userGroup);

        UserGroup NewProperties();

        UserGroup SaveProperties(UserGroup userGroup);

        MessageResult Remove(int id);

        MessageResult RemovePreAction(int id);

        CopyResult Copy(int id);

        UserGroupInitTreeResult InitTree(int parentId);
    }
}
