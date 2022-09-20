namespace Quantumart.QP8.BLL.Repository.ActiveDirectory
{
    public interface IActiveDirectoryRepository
    {
        /// <summary>
        /// Поиск групп в Active Directory
        /// </summary>
        /// <param name="groups">имена групп</param>
        /// <param name="membership">группа входит как минимум в одну из групп</param>
        /// <returns>группы</returns>
        ActiveDirectoryGroup[] GetGroups(string[] groups, params ActiveDirectoryGroup[] membership);

        /// <summary>
        /// Поиск пользователей в Active Directory
        /// </summary>
        /// <param name="membership">пользователь входит как минимум в одну из групп</param>
        /// <returns>пользователи</returns>
        ActiveDirectoryUser[] GetUsers(params ActiveDirectoryGroup[] membership);
    }
}
