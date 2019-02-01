#if !NET_STANDARD
using System.DirectoryServices;

namespace Quantumart.QP8.BLL.Repository.ActiveDirectory
{
    internal class ActiveDirectoryGroup : ActiveDirectoryEntityBase
    {
        public ActiveDirectoryGroup(SearchResult group)
            : base(group)
        {
        }
    }
}
#endif
