using System.Collections.Generic;

namespace Quantumart.QP8.EntityFramework6.Data
{
    public partial class UserGroup
    {
        public UserGroup()
        {
            Users = new HashSet<User>();
        }

        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
        public virtual ICollection<User> Users { get; set; }
    }
}
	
