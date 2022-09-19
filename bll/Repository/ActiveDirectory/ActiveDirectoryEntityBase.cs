using Novell.Directory.Ldap;

namespace Quantumart.QP8.BLL.Repository.ActiveDirectory
{
    public abstract class ActiveDirectoryEntityBase
    {
        public string ReferencedPath { get; }

        public string Name { get; }

        public string[] MemberOf { get; }

        protected ActiveDirectoryEntityBase(LdapEntry entry)
        {
            ReferencedPath = entry.Dn;
            Name = entry.GetAttribute("cn").StringValue;
            MemberOf = entry.GetAttribute("memberOf").StringValueArray;
        }
    }
}
