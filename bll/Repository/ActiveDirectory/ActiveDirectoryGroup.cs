using Novell.Directory.Ldap;

namespace Quantumart.QP8.BLL.Repository.ActiveDirectory
{
    public class ActiveDirectoryGroup : ActiveDirectoryEntityBase
    {
        public ActiveDirectoryGroup(LdapEntry group)
            : base(group)
        {
        }
    }
}
