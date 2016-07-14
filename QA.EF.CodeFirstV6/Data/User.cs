using System.Collections.Generic;

namespace QA.EF.CodeFirstV6.Data
{
    public partial class User
    {
        public int Id { get; set; }
        public string login { get; set; }
        public string NTLogin { get; set; }
        public string ISOCode { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public virtual ICollection<UserGroup> UserGroups { get; set; }
    }
}
