using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.Validators;
using Quantumart.QP8.Resources;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Repository.EntityPermissions;

namespace Quantumart.QP8.BLL
{
	/// <summary>
	/// Доступ к сущности
	/// </summary>
	public class EntityPermission : EntityPermissionBase
	{
		public static readonly int USER_MEMBER_TYPE = 1;
		public static readonly int GROUP_MEMBER_TYPE = 2;

		private IPermissionRepository repository;
		public IPermissionRepository Repository { get { return repository; } }

		#region props
		
		public User User { get; set; }
		
		public UserGroup Group { get; set; }
		
		public EntityPermissionLevel PermissionLevel { get; set; }

		[LocalizedDisplayName("PropagateToItems", NameResourceType = typeof(EntityPermissionStrings))]
		public bool PropagateToItems { get; set; }

		[LocalizedDisplayName("MemberType", NameResourceType = typeof(EntityPermissionStrings))]
		public int MemberType { get; set; }

		[LocalizedDisplayName("ExplicitPermissionToRelatedContents", NameResourceType = typeof(EntityPermissionStrings))]
		public bool ExplicitPermissionToRelatedContents { get; set; }

		[LocalizedDisplayName("Hide", NameResourceType = typeof(EntityPermissionStrings))]
		public bool Hide { get; set; }
		
		#endregion		

		internal static EntityPermission Create(int parentId, IPermissionRepository repository)
		{
			EntityPermission permission = new EntityPermission();
			permission.InitNew(parentId, repository);

			return permission;
		}

		private void InitNew(int parentId, IPermissionRepository repository)
		{
			if (IsNew)
				ParentEntityId = parentId;
			Init(repository);
		}

		public void Init(IPermissionRepository repository)
		{
			this.repository = repository;
			if (IsNew && !UserId.HasValue && !GroupId.HasValue)
			{
				MemberType = GROUP_MEMBER_TYPE;
				PermissionLevelId = CommonPermissionRepository.GetPermissionLevels()
					.Single(l => l.Level == Quantumart.QP8.Constants.PermissionLevel.Read)
					.Id;
			}
			else
			{
				if (UserId.HasValue)
					MemberType = USER_MEMBER_TYPE;
				else if (GroupId.HasValue)
					MemberType = GROUP_MEMBER_TYPE;
			}
		}

		public void DoCustomBinding()
		{
			if (MemberType == USER_MEMBER_TYPE)
				GroupId = null;
			else if (MemberType == GROUP_MEMBER_TYPE)
				UserId = null;
			else
			{
				GroupId = null;
				UserId = null;
			}

			if (IsNew)
			{
				if (MemberType == USER_MEMBER_TYPE && UserId.HasValue)
					User = UserRepository.GetById(UserId.Value, true);
				else if (MemberType == GROUP_MEMBER_TYPE && GroupId.HasValue)
					Group = UserGroupRepository.GetById(GroupId.Value);
			}
		}

		public override void Validate()
		{
			RulesException<EntityPermission> errors = new RulesException<EntityPermission>();
			base.Validate(errors);

			if (MemberType != USER_MEMBER_TYPE && MemberType != GROUP_MEMBER_TYPE)
				errors.ErrorForModel(EntityPermissionStrings.SystemError);

			if(MemberType == USER_MEMBER_TYPE && !UserId.HasValue)
				errors.ErrorFor(m => m.UserId, EntityPermissionStrings.UserIsNotSelected);
			if (MemberType == GROUP_MEMBER_TYPE && !GroupId.HasValue)
				errors.ErrorFor(m => m.GroupId, EntityPermissionStrings.GroupIsNotSelected);

			if (!errors.IsEmpty)
				throw errors;
		}

		protected override void ValidateUnique(RulesException errors)
		{
			if(IsNew && !repository.CheckUnique(this))
				errors.ErrorForModel(EntityPermissionStrings.PermissionIsNotUnique);
		}
	}
}
