#if !NET_STANDARD
using System;
using System.DirectoryServices;
using System.Linq;

namespace Quantumart.QP8.BLL.Repository.ActiveDirectory
{
    internal class ActiveDirectoryUser : ActiveDirectoryEntityBase
    {
        private readonly string AccountDescriptor = "DC=";

        public string FirstName { get; }
        public string LastName { get; }
        public string Mail { get; }
        public string AccountName { get; }
        public UserAccountControlDescription AccountControl { get; }
        public bool IsDisabled { get; }

        public ActiveDirectoryUser(SearchResult user)
            : base(user)
        {
            FirstName = GetValue<string>(user, "givenName");
            LastName = GetValue<string>(user, "sn");
            Mail = GetValue<string>(user, "mail");
            AccountName = GetDomain() + "\\" + GetValue<string>(user, "sAMAccountName");
            AccountControl = (UserAccountControlDescription)GetValue<int>(user, "userAccountControl");
            IsDisabled = (AccountControl & UserAccountControlDescription.ACCOUNTDISABLE) == UserAccountControlDescription.ACCOUNTDISABLE;
        }

        private string GetDomain()
        {
            return ReferencedPath.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(s => s.StartsWith(AccountDescriptor))
                .Select(s => s.Replace(AccountDescriptor, string.Empty))
                .FirstOrDefault();
        }
    }
}
#endif
