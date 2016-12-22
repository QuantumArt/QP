using System.Collections.Generic;

namespace EntityFramework6.Test.DataContext
{
    public partial class User
    {
        public User()
        {
            UserGroups = new HashSet<UserGroup>();
        }

        public virtual int Id { get; set; }
        public virtual string login { get; set; }
        public virtual string NTLogin { get; set; }
        public virtual string ISOCode { get; set; }
        public virtual string FirstName { get; set; }
        public virtual string LastName { get; set; }
        public virtual string Email { get; set; }
        public virtual ICollection<UserGroup> UserGroups { get; set; }
    }
}
