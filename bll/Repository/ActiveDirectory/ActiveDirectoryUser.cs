using System;
using System.DirectoryServices;
using System.Linq;

namespace Quantumart.QP8.BLL.Repository.ActiveDirectory
{
	internal class ActiveDirectoryUser : ActiveDirectoryEntityBase
	{
		private string AccountDescriptor = "DC=";

		public string FirstName { get; private set; }
		public string LastName { get; private set; }
		public string Mail { get; private set; }
		public string AccountName { get; private set; }
		public UserAccountControlDescription AccountControl { get; private set; }
		public bool IsDisabled { get; private set; }

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