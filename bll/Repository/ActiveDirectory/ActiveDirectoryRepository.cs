using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using Quantumart.QP8.Configuration;

namespace Quantumart.QP8.BLL.Repository.ActiveDirectory
{

    internal class ActiveDirectoryRepository
    {
        private readonly string[] _groupProperties = { "cn", "memberOf" };
        private readonly string[] _userProperties = { "cn", "memberOf", "sn", "givenName", "mail", "sAMAccountName", "userAccountControl" };
        private readonly string _path;
        private readonly string _userName;
        private readonly string _password;

        public ActiveDirectoryRepository()
        {
            _userName = QPConfiguration.ADsConnectionUsername;
            _password = QPConfiguration.ADsConnectionPassword;
            _path = QPConfiguration.ADsPath;
        }

        public ActiveDirectoryRepository(string path, string userName, string password)
        {
            _userName = userName;
            _password = password;
            _path = path;
        }

        /// <summary>
        /// Поиск групп в Active Directory
        /// </summary>
        /// <param name="groups">имена групп</param>
        /// <param name="membership">группа входит как минимум в одну из групп</param>
        /// <returns>группы</returns>
        public ActiveDirectoryGroup[] GetGroups(string[] groups, params ActiveDirectoryGroup[] membership)
        {
            using (var cnn = GetConnection())
            using (var search = new DirectorySearcher(cnn))
            {
                search.PropertiesToLoad.AddRange(_groupProperties);
                search.Filter = LdapQueryBuilder.GroupOf(groups, GetGroupReferences(membership));
                return search.FindAll().OfType<SearchResult>().Select(sr => new ActiveDirectoryGroup(sr)).ToArray();
            }
        }

        /// <summary>
        /// Поиск пользователей в Active Directory
        /// </summary>
        /// <param name="membership">пользователь входит как минимум в одну из групп</param>
        /// <returns>пользователи</returns>
        public ActiveDirectoryUser[] GetUsers(params ActiveDirectoryGroup[] membership)
        {
            using (var cnn = GetConnection())
            using (var search = new DirectorySearcher(cnn))
            {
                search.PropertiesToLoad.AddRange(_userProperties);
                search.Filter = LdapQueryBuilder.UserOf(GetGroupReferences(membership));
                return search.FindAll().OfType<SearchResult>().Select(sr => new ActiveDirectoryUser(sr)).ToArray();
            }
        }

        protected DirectoryEntry GetConnection() => new DirectoryEntry
        {
            Path = _path,
            AuthenticationType = AuthenticationTypes.Secure,
            Username = _userName,
            Password = _password
        };

        private static string[] GetGroupReferences(IEnumerable<ActiveDirectoryGroup> membership)
        {
            return membership.Select(g => g.ReferencedPath).ToArray();
        }
    }
}

