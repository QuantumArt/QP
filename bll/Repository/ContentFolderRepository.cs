using System.Collections.Generic;
using System.Linq;
using Quantumart.QP8.DAL;
using Quantumart.QP8.BLL.Mappers;
using System.Data.Objects;
using System;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Repository
{
	internal class ContentFolderRepository : FolderRepository
	{
		
		internal ObjectSet<ContentFolderDAL> CurrentSet
		{
			get
			{
				return QPContext.EFContext.ContentFolderSet;
			}
		}

		internal ContentFolderMapper CurrentMapper
		{
			get
			{
				return Mappers.MappersRepository.ContentFolderMapper;
			}
		}

		#region overrides

		public override Folder GetById(int id)
		{
			return CurrentMapper.GetBizObject(
				CurrentSet.Include("LastModifiedByUser")
					.Where(s => s.Id == id)
					.SingleOrDefault()
			);
		}

		public override Folder GetRoot(int parentEntityId)
		{
			return CurrentMapper.GetBizObject(
				CurrentSet
					.Where(s => s.ContentId == (decimal)parentEntityId && s.ParentId == null)
					.SingleOrDefault()
			);
		}

		public override Folder GetChildByName(int parentId, string name)
		{
			return CurrentMapper.GetBizObject(
				CurrentSet
					.Where(s => s.ParentId == parentId && s.Name == name)
					.SingleOrDefault()
			);
		}

		public override Folder CreateInDB(Folder folder)
		{
			return DefaultRepository.Save<ContentFolder, ContentFolderDAL>((ContentFolder)folder);
		}

		public override Folder CreateInDBAsAdmin(Folder folder)
		{
			return DefaultRepository.SaveAsAdmin<ContentFolder, ContentFolderDAL>((ContentFolder)folder);
		}

		public override IEnumerable<Folder> GetAllChildrenFromDB(int parentId)
		{
			return CurrentMapper.GetBizList(
				CurrentSet
					.Where(c => c.ParentId == (decimal)parentId)
					.ToList()
			);
		}

		public override IEnumerable<Folder> GetChildrenFromDB(int parentEntityId, int parentId)
		{
			int totalRecords = -1;
			return CurrentMapper.GetBizList(
				QPContext.EFContext.GetChildContentFoldersList(
					QPContext.CurrentUserId, parentEntityId, parentId, Constants.PermissionLevel.List, false, out totalRecords
				)
			);
		}

		public override IEnumerable<Folder> GetChildrenWithSync(int parentEntityId, int? parentId)
		{
			int newId = Synchronize(parentEntityId, parentId);
			return GetChildrenFromDB(parentEntityId, newId);
		}

		public override List<PathSecurityInfo> GetPaths(int contentId)
		{
			return CurrentSet.Where(n => n.ContentId == contentId).Select(n => new PathSecurityInfo { Id = (int)n.Id, Path = n.Path }).ToList();
		}

		public override string FolderNotFoundMessage(int id)
		{
			return String.Format(LibraryStrings.ContentFolderNotExists);
		}

		#endregion


		protected override Folder UpdateInDb(Folder folder)
		{			
			using (var scope = new QPConnectionScope())
			{
				Common.UpdateContentSubFoldersPath(scope.DbConnection, folder.Id, folder.Path, QPContext.CurrentUserId, DateTime.Now);
			}
			return DefaultRepository.Update<ContentFolder, ContentFolderDAL>((ContentFolder)folder);
		}

		protected override void DeleteFromDb(Folder folder)
		{			
			Action<Folder> traverse = null;
			traverse = (f) =>
			{
				f.LoadAllChildren = true;
				f.AutoLoadChildren = true;
				if (f.Children != null)
				{
					foreach (var sb in f.Children)
					{
						traverse((Folder)sb);
					}
				}
				DefaultRepository.Delete<ContentFolderDAL>(f.Id);
			};
			traverse(folder);
		}
	}
}
