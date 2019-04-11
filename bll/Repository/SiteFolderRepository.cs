using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Quantumart.QP8.BLL.Facades;
using Quantumart.QP8.BLL.Mappers;
using Quantumart.QP8.Constants;
using Quantumart.QP8.DAL;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Repository
{
    internal class SiteFolderRepository : FolderRepository
    {
        internal DbSet<SiteFolderDAL> CurrentSet => QPContext.EFContext.SiteFolderSet;

        internal SiteFolderMapper CurrentMapper => MapperFacade.SiteFolderMapper;

        internal SiteFolderRowMapper RowMapper = MapperFacade.SiteFolderRowMapper;

        public override Folder GetById(int id)
        {
            return CurrentMapper.GetBizObject(CurrentSet.Include("LastModifiedByUser").SingleOrDefault(s => s.Id == id));
        }

        public override Folder GetChildByName(int parentId, string name)
        {
            return CurrentMapper.GetBizObject(CurrentSet.SingleOrDefault(s => s.ParentId == parentId && s.Name == name));
        }

        public override Folder GetRoot(int parentEntityId)
        {
            return CurrentMapper.GetBizObject(CurrentSet.SingleOrDefault(s => s.SiteId == parentEntityId && s.ParentId == null));
        }

        public override Folder CreateInDb(Folder folder) => DefaultRepository.Save<SiteFolder, SiteFolderDAL>((SiteFolder)folder);

        public override Folder CreateInDbAsAdmin(Folder folder) => DefaultRepository.SaveAsAdmin<SiteFolder, SiteFolderDAL>((SiteFolder)folder);

        public override IEnumerable<Folder> GetAllChildrenFromDb(int parentId)
        {
            return CurrentMapper.GetBizList(CurrentSet.Where(c => c.ParentId == parentId).ToList());
        }

        public override IEnumerable<Folder> GetChildrenFromDb(int parentEntityId, int parentId)
        {
            using (var scope = new QPConnectionScope())
            {
                return RowMapper.GetBizList(
                    Common.GetChildFoldersList(scope.DbConnection, QPContext.CurrentUserId,
                        parentEntityId, true, parentId, PermissionLevel.List, false, out var totalRecords)
                        .ToList()
                );
            }
        }

        public override IEnumerable<Folder> GetChildrenWithSync(int parentEntityId, int? parentId)
        {
            var newId = Synchronize(parentEntityId, parentId);
            return GetChildrenFromDb(parentEntityId, newId);
        }

        public override List<PathSecurityInfo> GetPaths(int siteId)
        {
            return CurrentSet.Where(n => n.SiteId == siteId).Select(n => new PathSecurityInfo { Id = (int)n.Id, Path = n.Path }).ToList();
        }

        public override string FolderNotFoundMessage(int id) => string.Format(LibraryStrings.SiteFolderNotExists);

        protected override Folder UpdateInDb(Folder folder)
        {
            using (var scope = new QPConnectionScope())
            {
                Common.UpdateSiteSubFoldersPath(scope.DbConnection, folder.Id, folder.Path, QPContext.CurrentUserId, DateTime.Now);
            }

            return DefaultRepository.Update<SiteFolder, SiteFolderDAL>((SiteFolder)folder);
        }

        protected override void DeleteFromDb(Folder folder)
        {
            Action<Folder> traverse = null;
            traverse = f =>
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

                DefaultRepository.Delete<SiteFolderDAL>(f.Id);
            };

            traverse(folder);
        }
    }
}
