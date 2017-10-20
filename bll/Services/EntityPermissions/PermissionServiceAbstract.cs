using System;
using System.Collections.Generic;
using System.Linq;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Repository.Articles;
using Quantumart.QP8.BLL.Repository.EntityPermissions;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Services.EntityPermissions
{
	public abstract class PermissionServiceAbstract : IPermissionService
	{
		public abstract IPermissionRepository Repository { get; }
 
		#region IPermissionService Members		
		public abstract IPermissionListViewModelSettings ListViewModelSettings { get; }
		public abstract IPermissionViewModelSettings ViewModelSettings { get; }
		
		public IEnumerable<EntityPermissionLevel> GetPermissionLevels() => CommonPermissionRepository.GetPermissionLevels();

	    public virtual ListResult<EntityPermissionListItem> List(int parentId, ListCommand cmd)
		{
            var list = Repository.List(cmd, parentId, out var totalRecords);
            return new ListResult<EntityPermissionListItem>
			{
				Data = list.ToList(),
				TotalRecords = totalRecords
			};
		}

		public virtual EntityPermission Read(int id)
		{
			var permission = Repository.GetById(id);
			if (permission == null)
			{
			    throw new ApplicationException(string.Format(EntityPermissionStrings.PermissionNotFound, id));
			}

		    return permission;
		}

		public EntityPermission ReadForUpdate(int id)
		{
			var permission = Repository.GetById(id, false);
			if (permission == null)
			{
			    throw new ApplicationException(string.Format(EntityPermissionStrings.PermissionNotFound, id));
			}

		    return permission;
		}

		public virtual EntityPermission New(int parentId) => EntityPermission.Create(parentId, Repository);

	    public virtual EntityPermission Save(EntityPermission permission) => Repository.Save(permission);

	    public virtual EntityPermission Update(EntityPermission permission) => Repository.Update(permission);

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

		public Article GetParentArticle(int articleId) => ArticleRepository.GetById(articleId);

	    public virtual PermissionInitListResult InitList(int parentId) => new PermissionInitListResult
		{
		    IsAddNewAccessable = SecurityRepository.IsActionAccessible(ListViewModelSettings.AddNewItemActionCode)				
		};

	    #endregion
	}
}
