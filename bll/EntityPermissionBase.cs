using Quantumart.QP8.Resources;
using Quantumart.QP8.Validators;

namespace Quantumart.QP8.BLL
{
	public class EntityPermissionBase : EntityObject
	{
		[LocalizedDisplayName("User", NameResourceType = typeof(EntityPermissionStrings))]
		public int? UserId { get; set; }
		[LocalizedDisplayName("Group", NameResourceType = typeof(EntityPermissionStrings))]
		public int? GroupId { get; set; }

		[LocalizedDisplayName("PermissionLevel", NameResourceType = typeof(EntityPermissionStrings))]
		public int PermissionLevelId { get; set; }

		public override int ParentEntityId { get; set; }

		public override string Name { get { return null; } set { } }
	}
}
