using System.ComponentModel.DataAnnotations;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL
{
    public class EntityPermissionBase : EntityObject
    {
        [Display(Name = "User", ResourceType = typeof(EntityPermissionStrings))]
        public int? UserId { get; set; }

        [Display(Name = "Group", ResourceType = typeof(EntityPermissionStrings))]
        public int? GroupId { get; set; }

        [Display(Name = "PermissionLevel", ResourceType = typeof(EntityPermissionStrings))]
        public int PermissionLevelId { get; set; }

        public override int ParentEntityId { get; set; }

        public override string Name
        {
            get { return null; }
            set { }
        }
    }
}
