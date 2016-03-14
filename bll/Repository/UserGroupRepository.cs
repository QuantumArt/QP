using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.BLL.Mappers;
using Quantumart.QP8.BLL.Services.DTO;
using System.Data;
using Quantumart.QP8.DAL;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Utils;
using Quantumart.QP8.BLL.Helpers;

namespace Quantumart.QP8.BLL.Repository
{
	internal static class UserGroupRepository
	{
		internal static IEnumerable<UserGroup> GetAllGroups()
		{
			return MappersRepository.UserGroupMapper.GetBizList(QPContext.EFContext.UserGroupSet.ToList());
		}

		internal static IEnumerable<UserGroupListItem> List(ListCommand cmd, out int totalRecords, IEnumerable<int> selectedIDs)
		{
			using (var scope = new QPConnectionScope())
			{
				cmd.SortExpression = !String.IsNullOrWhiteSpace(cmd.SortExpression) ? TranslateHelper.TranslateSortExpression(cmd.SortExpression) : null;
				IEnumerable<DataRow> rows = Common.GetUserGroupPage(scope.DbConnection,	selectedIDs, cmd.SortExpression, cmd.FilterExpression, cmd.StartRecord, cmd.PageSize, out totalRecords);
				return MappersRepository.UserGroupListItemRowMapper.GetBizList(rows.ToList());
			}
		}

		/// <summary>
		/// Возвращает список по ids
		/// </summary>
		/// <returns></returns>
		internal static IEnumerable<UserGroup> GetList(IEnumerable<int> IDs)
		{
			IEnumerable<decimal> decIDs = Converter.ToDecimalCollection(IDs).Distinct().ToArray();
			return MappersRepository.UserGroupMapper
				.GetBizList(QPContext.EFContext.UserGroupSet
					.Where(f => decIDs.Contains(f.Id))
					.ToList()
				);
		}

		internal static UserGroup GetPropertiesById(int id)
		{
			return MappersRepository.UserGroupMapper.GetBizObject(QPContext.EFContext.UserGroupSet
				.Include("ParentGroups")
				.Include("ChildGroups")
				.Include("Users")
				.Include("LastModifiedByUser")
				.SingleOrDefault(g => g.Id == id)
			);
		}

		internal static UserGroup GetById(int id)
		{
			return MappersRepository.UserGroupMapper.GetBizObject(QPContext.EFContext.UserGroupSet.SingleOrDefault(g => g.Id == id));
		}

		internal static UserGroup UpdateProperties(UserGroup group)
		{
			QP8Entities entities = QPContext.EFContext;
			UserGroupDAL dal = MappersRepository.UserGroupMapper.GetDalObject(group);
			dal.LastModifiedBy = QPContext.CurrentUserId;
			using (new QPConnectionScope())
			{
				dal.Modified = Common.GetSqlDate(QPConnectionScope.Current.DbConnection);
			}
			entities.UserGroupSet.Attach(dal);
			entities.ObjectStateManager.ChangeObjectState(dal, EntityState.Modified);

			UserGroupDAL dalDb = entities.UserGroupSet
				.Include("ParentGroups")
				.Include("Users")
				.Single(g => g.Id == dal.Id);

			// Parent group
			foreach(var pg in dalDb.ParentGroups.ToArray())
			{
				if (group.ParentGroup == null || pg.Id != group.ParentGroup.Id)
				{
					entities.UserGroupSet.Attach(pg);
					dalDb.ParentGroups.Remove(pg);
				}
			}
			if (group.ParentGroup != null)
			{
				if (!dalDb.ParentGroups.Where(g => g.Id == group.ParentGroup.Id).Any())
				{
					UserGroupDAL dalParent = entities.UserGroupSet.Single(g => g.Id == group.ParentGroup.Id);
					entities.UserGroupSet.Attach(dalParent);
					dal.ParentGroups.Add(dalParent);
				}
			}
			//------------------

			//users
			HashSet<decimal> inmemoryUserIDs = new HashSet<decimal>(group.Users.Select(u => Converter.ToDecimal(u.Id)));
			HashSet<decimal> indbUserIDs = new HashSet<decimal>(dalDb.Users.Select(u => u.Id));
			foreach (var u in dalDb.Users.ToArray())
			{
				if (!inmemoryUserIDs.Contains(u.Id))
				{
					entities.UserSet.Attach(u);
					dalDb.Users.Remove(u);
				}
			}
			foreach (var u in MappersRepository.UserMapper.GetDalList(group.Users.ToList()))
			{
				if (!indbUserIDs.Contains(u.Id))
				{
					entities.UserSet.Attach(u);
					dal.Users.Add(u);
				}
			}
			//-------------


			entities.SaveChanges();


			return MappersRepository.UserGroupMapper.GetBizObject(dal);
		}

		internal static UserGroup SaveProperties(UserGroup group)
		{
			QP8Entities entities = QPContext.EFContext;
			UserGroupDAL dal = MappersRepository.UserGroupMapper.GetDalObject(group);
			dal.LastModifiedBy = QPContext.CurrentUserId;
			using (new QPConnectionScope())
			{
				dal.Created  = Common.GetSqlDate(QPConnectionScope.Current.DbConnection);
				dal.Modified = dal.Created;
			}
			entities.UserGroupSet.AddObject(dal);
			entities.SaveChanges();

			if (group.ParentGroup != null)
			{
				UserGroupDAL parentDal = MappersRepository.UserGroupMapper.GetDalObject(group.ParentGroup);
				entities.UserGroupSet.Attach(parentDal);
				dal.ParentGroups.Add(parentDal);
			}

			foreach (var u in MappersRepository.UserMapper.GetDalList(group.Users.ToList()))
			{
				entities.UserSet.Attach(u);
				dal.Users.Add(u);
			}

			entities.SaveChanges();
			return MappersRepository.UserGroupMapper.GetBizObject(dal);
		}

		/// <summary>
		/// Отфильтровать пользователей прямо или косвенно входящих в группу администраторов 
		/// </summary>
		/// <param name="p"></param>
		/// <returns></returns>
		internal static IEnumerable<int> SelectAdminDescendantGroupUserIDs(IEnumerable<int> userIds, int groupId)
		{
			using (var scope = new QPConnectionScope())
			{
				return Common.UserGroups_SelectAdminDescendantGroupUserIDs(userIds, groupId, scope.DbConnection);
			}
		}

		/// <summary>
		/// Входит ли группа в иерархию группы Администраторы
		/// </summary>
		/// <param name="Id"></param>
		/// <returns></returns>
		internal static bool IsGroupAdminDescendant(int groupId)
		{
			using (var scope = new QPConnectionScope())
			{
				return Common.UserGroups_IsGroupAdminDescendant(groupId, scope.DbConnection);
			}
		}

		/// <summary>
		/// Проверит возможность образования цикла в иерархии групп
		/// </summary>
		/// <param name="groupId"></param>
		/// <param name="parentGroupId"></param>
		/// <returns></returns>
		internal static bool IsCyclePossible(int groupId, int parentGroupId)
		{
			using (var scope = new QPConnectionScope())
			{
				return Common.UserGroups_IsCyclePossible(groupId, parentGroupId, scope.DbConnection);
			}
		}

		/// <summary>
		/// Отфильтровать пользователей прямо входящих в группы использующие параллельный Workflow
		/// </summary>
		/// <param name="p"></param>
		/// <returns></returns>
		internal static IEnumerable<int> SelectWorkflowGroupUserIDs(int[] userIds)
		{
			using (var scope = new QPConnectionScope())
			{
				return Common.UserGroups_SelectWorkflowGroupUserIDs(userIds, scope.DbConnection);
			}
		}

		/// <summary>
		/// Возвращает иерархию группы Администраторы
		/// </summary>
		/// <returns></returns>
		internal static IEnumerable<DataRow> GetAdministratorsHierarhy()
		{
			using (var scope = new QPConnectionScope())
			{
				return Common.UserGroups_GetAdministratorsHierarhy(scope.DbConnection);
			}
		}

		internal static int CopyGroup(UserGroup group, int currentUserId)
		{
			using (var scope = new QPConnectionScope())
			{
				return Common.CopyUserGroup(group.Id, group.Name, currentUserId, scope.DbConnection);
			}
		}

		internal static IEnumerable<ListItem> GetSimpleList(int[] groupIDs)
		{
			return GetAllGroups()
				.Where(g => groupIDs.Contains(g.Id))
				.Select(g => new ListItem { Value = g.Id.ToString(), Text = g.Name })
				.ToArray();
		}

		internal static void Delete(int id)
		{
			DefaultRepository.Delete<UserGroupDAL>(id);
		}
	}
}
