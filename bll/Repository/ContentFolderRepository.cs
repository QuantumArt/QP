using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Linq;
using Quantumart.QP8.BLL.Mappers;
using Quantumart.QP8.Constants;
using Quantumart.QP8.DAL;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Repository
{
    internal class ContentFolderRepository : FolderRepository
    {
        internal ObjectSet<ContentFolderDAL> CurrentSet => QPContext.EFContext.ContentFolderSet;

        internal ContentFolderMapper CurrentMapper => MappersRepository.ContentFolderMapper;

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

        public override Folder CreateInDb(Folder folder)
        {
            return DefaultRepository.Save<ContentFolder, ContentFolderDAL>((ContentFolder)folder);
        }

        public override Folder CreateInDbAsAdmin(Folder folder)
        {
            return DefaultRepository.SaveAsAdmin<ContentFolder, ContentFolderDAL>((ContentFolder)folder);
        }

        public override IEnumerable<Folder> GetAllChildrenFromDb(int parentId)
        {
            return CurrentMapper.GetBizList(CurrentSet.Where(c => c.ParentId == parentId).ToList());
        }

        public override IEnumerable<Folder> GetChildrenFromDb(int parentEntityId, int parentId)
        {
            int totalRecords;
            return CurrentMapper.GetBizList(QPContext.EFContext.GetChildContentFoldersList(QPContext.CurrentUserId, parentEntityId, parentId, PermissionLevel.List, false, out totalRecords));
        }

        public override IEnumerable<Folder> GetChildrenWithSync(int parentEntityId, int? parentId)
        {
            var newId = Synchronize(parentEntityId, parentId);
            return GetChildrenFromDb(parentEntityId, newId);
        }

        public override List<PathSecurityInfo> GetPaths(int contentId)
        {
            return CurrentSet.Where(n => n.ContentId == contentId).Select(n => new PathSecurityInfo { Id = (int)n.Id, Path = n.Path }).ToList();
        }

        public override string FolderNotFoundMessage(int id)
        {
            return string.Format(LibraryStrings.ContentFolderNotExists);
        }

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

                DefaultRepository.Delete<ContentFolderDAL>(f.Id);
            };

            traverse(folder);
        }
    }
}
