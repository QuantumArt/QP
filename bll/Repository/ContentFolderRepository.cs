using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Quantumart.QP8.BLL.Facades;
using Quantumart.QP8.BLL.Mappers;
using Quantumart.QP8.Constants;
using Quantumart.QP8.DAL;
using Quantumart.QP8.Resources;
using ContentFolderDAL = Quantumart.QP8.DAL.Entities.ContentFolderDAL;

namespace Quantumart.QP8.BLL.Repository
{
    internal class ContentFolderRepository : FolderRepository
    {
        internal DbSet<ContentFolderDAL> CurrentSet => QPContext.EFContext.ContentFolderSet;

        internal ContentFolderMapper CurrentMapper => MapperFacade.ContentFolderMapper;

        internal ContentFolderRowMapper RowMapper => MapperFacade.ContentFolderRowMapper;

        public override Folder GetById(int id)
        {
            return CurrentMapper.GetBizObject(CurrentSet.Include("LastModifiedByUser").SingleOrDefault(s => s.Id == id));
        }

        public override Folder GetRoot(int parentEntityId)
        {
            return CurrentMapper.GetBizObject(CurrentSet.SingleOrDefault(s => s.ContentId == (decimal)parentEntityId && s.ParentId == null));
        }

        public override Folder GetChildByName(int parentId, string name)
        {
            return CurrentMapper.GetBizObject(CurrentSet.SingleOrDefault(s => s.ParentId == parentId && s.Name == name));
        }

        public override Folder CreateInDb(Folder folder) => DefaultRepository.Save<ContentFolder, ContentFolderDAL>((ContentFolder)folder);

        public override Folder CreateInDbAsAdmin(Folder folder) => DefaultRepository.SaveAsAdmin<ContentFolder, ContentFolderDAL>((ContentFolder)folder);

        public override IEnumerable<Folder> GetAllChildrenFromDb(int parentId)
        {
            return CurrentMapper.GetBizList(CurrentSet.Where(c => c.ParentId == parentId).ToList());
        }

        public override IEnumerable<Folder> GetChildrenFromDb(int parentEntityId, int parentId)
        {
            using (var scope = new QPConnectionScope())
            {

                return RowMapper.GetBizList(
                    Common.GetChildFoldersList(
                        scope.DbConnection, QPContext.EFContext, QPContext.IsAdmin, QPContext.CurrentUserId,
                        parentEntityId,false, parentId, PermissionLevel.List, false, out int _)
                        .ToList()
                );
            }
        }

        public override IEnumerable<Folder> GetChildrenWithSync(int parentEntityId, int? parentId)
        {
            var newId = Synchronize(parentEntityId, parentId);
            return GetChildrenFromDb(parentEntityId, newId);
        }

        public override List<PathSecurityInfo> GetPaths(int contentId)
        {
            return CurrentSet.Where(n => n.ContentId == contentId)
                .Select(n => new PathSecurityInfo
                {
                    Id = (int)n.Id,
                    Path = n.Path.Replace('\\', System.IO.Path.DirectorySeparatorChar)
                }).ToList();
        }

        public override string FolderNotFoundMessage(int id) => string.Format(LibraryStrings.ContentFolderNotExists);

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
            void Traverse(Folder f)
            {
                f.LoadAllChildren = true;
                f.AutoLoadChildren = true;
                if (f.Children != null)
                {
                    foreach (var sb in f.Children)
                    {
                        Traverse((Folder)sb);
                    }
                }

                DefaultRepository.Delete<ContentFolderDAL>(f.Id);
            }

            Traverse(folder);
        }
    }
}
