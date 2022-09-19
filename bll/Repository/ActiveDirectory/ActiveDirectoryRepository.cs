using System.Collections.Generic;
using System.Linq;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Security.Ldap;

namespace Quantumart.QP8.BLL.Repository.ActiveDirectory
{

    public class ActiveDirectoryRepository : IActiveDirectoryRepository
    {
        private readonly string[] _groupProperties = { "cn", "memberOf" };
        private readonly string[] _userProperties = { "cn", "memberOf", "sn", "givenName", "mail", "sAMAccountName", "userAccountControl" };
        private readonly string _path;
        private readonly string _userName;
        private readonly string _password;
        private readonly ILdapIdentityManager _ldapIdentityManager;

        public ActiveDirectoryRepository(ILdapIdentityManager ldapIdentityManager)
        {
            _userName = QPConfiguration.ADsConnectionUsername;
            _password = QPConfiguration.ADsConnectionPassword;
            _path = QPConfiguration.ADsPath;
            _ldapIdentityManager = ldapIdentityManager;
        }

        /// <summary>
        /// Поиск групп в Active Directory
        /// </summary>
        /// <param name="groups">имена групп</param>
        /// <param name="membership">группа входит как минимум в одну из групп</param>
        /// <returns>группы</returns>
        public ActiveDirectoryGroup[] GetGroups(string[] groups, params ActiveDirectoryGroup[] membership)
        {
            string groupFilter = LdapQueryBuilder.GroupOf(groups, GetGroupReferences(membership));
            List<Novell.Directory.Ldap.LdapEntry> groupList = _ldapIdentityManager.GetEntriesWithAttributesByFilter(groupFilter, _groupProperties);
            return groupList.Select(g => new ActiveDirectoryGroup(g)).ToArray();
        }

        /// <summary>
        /// Поиск пользователей в Active Directory
        /// </summary>
        /// <param name="membership">пользователь входит как минимум в одну из групп</param>
        /// <returns>пользователи</returns>
        public ActiveDirectoryUser[] GetUsers(params ActiveDirectoryGroup[] membership)
        {
            string userFilter = LdapQueryBuilder.UserOf(GetGroupReferences(membership));
            List<Novell.Directory.Ldap.LdapEntry> users = _ldapIdentityManager.GetEntriesWithAttributesByFilter(userFilter, _userProperties);
            return users.Select(u => new ActiveDirectoryUser(u)).ToArray();
        }

        private static string[] GetGroupReferences(IEnumerable<ActiveDirectoryGroup> membership)
        {
            return membership.Select(g => g.ReferencedPath).ToArray();
        }
    }
}

