using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quantumart.QP8.EntityFramework6.DevData
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
	
