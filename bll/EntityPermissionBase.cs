using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
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

        [ValidateNever]
        [BindNever]
        public override string Name
        {
            get { return null; }
            set { }
        }
    }
}
