using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.BLL.Repository.EntityPermissions;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.Resources;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Repository.Articles;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.BLL.Services.EntityPermissions
{
	public abstract class PermissionServiceAbstract : IPermissionService
	{
		public abstract IPermissionRepository Repository { get; }
 
		#region IPermissionService Members		
		public abstract IPermissionListViewModelSettings ListViewModelSettings { get; }
		public abstract IPermissionViewModelSettings ViewModelSettings { get; }
		
		public IEnumerable<EntityPermissionLevel> GetPermissionLevels()
		{
			return CommonPermissionRepository.GetPermissionLevels();
		}

		public virtual ListResult<EntityPermissionListItem> List(int parentId, ListCommand cmd)
		{
			int totalRecords;
			IEnumerable<EntityPermissionListItem> list = Repository.List(cmd, parentId, out totalRecords);
			return new ListResult<EntityPermissionListItem>
			{
				Data = list.ToList(),
				TotalRecords = totalRecords
			};
		}

		public virtual EntityPermission Read(int id)
		{
			EntityPermission permission = Repository.GetById(id);
			if (permission == null)
				throw new ApplicationException(String.Format(EntityPermissionStrings.PermissionNotFound, id));
			return permission;
		}

		public EntityPermission ReadForUpdate(int id)
		{
			EntityPermission permission = Repository.GetById(id, false);
			if (permission == null)
				throw new ApplicationException(String.Format(EntityPermissionStrings.PermissionNotFound, id));
			return permission;
		}

		public virtual EntityPermission New(int parentId)
		{
			return EntityPermission.Create(parentId, this.Repository);
		}

		public virtual EntityPermission Save(EntityPermission permission)
		{
			return Repository.Save(permission);
		}

		public virtual EntityPermission Update(EntityPermission permission)
		{
			return Repository.Update(permission);
		}

		public virtual MessageResult Remove(int parentId, int id)
		{
			Repository.Remove(id);
			return null;
		}

		public virtual MessageResult MultipleRemove(int parentId, IEnumerable<int> IDs)
		{
			Repository.MultipleRemove(IDs);
			return null;
		}

		public Article GetParentArticle(int articleId)
		{
			return ArticleRepository.GetById(articleId);
		}
		

		public virtual PermissionInitListResult InitList(int parentId)
		{
			return new PermissionInitListResult
			{
				IsAddNewAccessable = SecurityRepository.IsActionAccessible(this.ListViewModelSettings.AddNewItemActionCode),				
			};
		}

		#endregion
	}
}
