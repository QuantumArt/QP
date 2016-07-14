using System.Collections.Generic;

namespace QA.EF.CodeFirstV6.Data
{
    public partial class UserGroup
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual ICollection<User> Users { get; set; }
    }
}
