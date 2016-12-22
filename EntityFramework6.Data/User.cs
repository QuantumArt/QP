using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quantumart.QP8.EntityFramework6.Data
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
