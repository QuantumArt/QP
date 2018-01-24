using System.DirectoryServices;
using System.Linq;

namespace Quantumart.QP8.BLL.Repository.ActiveDirectory
{
    internal abstract class ActiveDirectoryEntityBase
    {
        public string Path { get; }

        public string ReferencedPath { get; }

        public string Name { get; }

        public string[] MemberOf { get; }

        protected ActiveDirectoryEntityBase(SearchResult entity)
        {
            Path = entity.Path;
            ReferencedPath = Path.Replace("LDAP://", string.Empty);
            Name = GetValue<string>(entity, "cn");
            MemberOf = entity.Properties["memberOf"].OfType<string>().ToArray();
        }

        protected T GetValue<T>(SearchResult entity, string key) => entity.Properties[key].OfType<T>().FirstOrDefault();
    }
}
