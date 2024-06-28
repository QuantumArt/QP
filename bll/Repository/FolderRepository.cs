using System;
using System.Collections.Generic;
using Quantumart.QP8.BLL.Factories.FolderFactory;
using Quantumart.QP8.BLL.Helpers;

namespace Quantumart.QP8.BLL.Repository
{
    public abstract class FolderRepository
    {
        private Folder GetOrCreateRoot(int parentEntityId, PathHelper pathHelper)
        {
            var folder = GetRoot(parentEntityId);
            if (folder == null)
            {
                folder = CreateRoot(parentEntityId, pathHelper, true);
            }
            else
            {
                if (!pathHelper.UseS3)
                {
                    folder.CreateInFs();
                }
            }

            return folder;
        }

        private Folder CreateRoot(int parentEntityId, PathHelper pathHelper, bool asAdmin = false)
        {
            return Create(parentEntityId, null, string.Empty, pathHelper, asAdmin);
        }

        private Folder Get(int parentEntityId, int? parentId, PathHelper pathHelper)
        {
            Folder folder;
            if (parentId == null)
            {
                folder = GetOrCreateRoot(parentEntityId, pathHelper);
            }
            else
            {
                folder = GetById((int)parentId);
                if (folder == null)
                {
                    throw new ArgumentException(string.Format(FolderNotFoundMessage(parentId.Value)));
                }
            }

            if (!pathHelper.UseS3)
            {
                folder.CreateInFs();
            }
            folder.AutoLoadChildren = true;
            return folder;
        }

        protected int Synchronize(int parentEntityId, int? parentId, PathHelper pathHelper)
        {
            var folder = Get(parentEntityId, parentId, pathHelper);
            folder.LoadAllChildren = true;
            folder.CreateChildren(pathHelper);
            return folder.Id;
        }

        public Folder GetSelfAndChildrenWithSync(int parentEntityId, int? parentId, PathHelper pathHelper)
        {
            Synchronize(parentEntityId, parentId, pathHelper);
            var folder = Get(parentEntityId, parentId, pathHelper);
            return folder;
        }

        public FolderFactory Factory { get; set; }

        public abstract string FolderNotFoundMessage(int id);

        /// <summary>
        /// Возвращает информацию о папке библиотеки
        /// </summary>
        /// <param name="id">ID папки</param>
        /// <returns>папка или null</returns>
        public abstract Folder GetById(int id);

        /// <summary>
        /// Возвращает информацию о дочерней папке библиотеки
        /// </summary>
        /// <param name="parentId">ID родительской папки</param>
        /// <param name="name">имя дочерней папки</param>
        /// <returns>папка или null</returns>
        public abstract Folder GetChildByName(int parentId, string name);

        /// <summary>
        /// Возвращает информацию о корневой папке библиотеки
        /// </summary>
        /// <param name="parentEntityId">ID родительской сущности (сайта или контента)</param>
        /// <returns>папка или null</returns>
        public abstract Folder GetRoot(int parentEntityId);

        /// <summary>
        /// Создает новую папку в БД
        /// </summary>
        /// <param name="folder">папка</param>
        /// <returns>сохраненная папка (имеет ID)</returns>
        public abstract Folder CreateInDb(Folder folder);

        /// <summary>
        /// Создает новую папку в БД от имени администратора
        /// </summary>
        /// <param name="folder">папка</param>
        /// <returns>сохраненная папка (имеет ID)</returns>
        public abstract Folder CreateInDbAsAdmin(Folder folder);

        /// <summary>
        /// Обновляет папку в БД
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        protected abstract Folder UpdateInDb(Folder folder);

        /// <summary>
        /// Удаляет папку из БД
        /// </summary>
        /// <param name="folder"></param>
        protected abstract void DeleteFromDb(Folder folder);

        /// <summary>
        /// Загружает все дочерние папки из базы данных
        /// </summary>
        /// <param name="parentId">ID папки</param>
        /// <returns>список дочерних папок</returns>
        public abstract IEnumerable<Folder> GetAllChildrenFromDb(int parentId);

        /// <summary>
        /// Загружает дочерние папки из базы данных
        /// </summary>
        /// <param name="parentEntityId">ID родительской сущности (сайта или контента)</param>
        /// <param name="parentId">ID папки</param>
        /// <returns>список дочерних папок</returns>
        public abstract IEnumerable<Folder> GetChildrenFromDb(int parentEntityId, int parentId);

        /// <summary>
        /// Получает дочерние папки с учетом прав доступа текущего пользователя. При этом выполняется синхронизация.
        /// </summary>
        /// <param name="parentEntityId">ID родительской сущности (сайта или контента)</param>
        /// <param name="parentId">ID папки</param>
        /// <param name="pathHelper"></param>
        /// <returns>список дочерних папок</returns>
        public abstract IEnumerable<Folder> GetChildrenWithSync(int parentEntityId, int? parentId, PathHelper pathHelper);

        /// <summary>
        /// Получает информацию, необходимую для проверки security путей
        /// </summary>
        /// <param name="parentEntityId">ID родительской сущности (сайта или контента)</param>
        /// <returns>список</returns>
        public abstract List<PathSecurityInfo> GetPaths(int parentEntityId);

        /// <summary>
        /// Получает информацию о папке библиотеки по относительному пути
        /// </summary>
        /// <param name="parentEntityId">ID родительской сущности (сайта или контента)</param>
        /// <param name="subFolder">относительный путь от корня библиотеки (сайта или контента)</param>
        /// <param name="pathHelper"></param>
        /// <returns>папка</returns>
        public Folder GetBySubFolder(int parentEntityId, string subFolder, PathHelper pathHelper)
        {
            return GetOrCreateRoot(parentEntityId, pathHelper).TraverseTree(subFolder, pathHelper);
        }

        /// <summary>
        /// Создание новой папки (в БД и в файловой системе)
        /// </summary>
        /// <param name="parentEntityId">ID родительской сущности(сайта или контента)</param>
        /// <param name="parentId">ID родительской папки</param>
        /// <param name="name">имя папки</param>
        /// <param name="pathHelper"></param>
        /// <param name="asAdmin">признак, создавать ли папку от имени админа (необходимо для фонового создания папок)</param>
        /// <returns></returns>
        public Folder Create(int parentEntityId, int? parentId, string name, PathHelper pathHelper, bool asAdmin = false)
        {
            var folder = Factory.CreateFolder();
            folder.ParentEntityId = parentEntityId;
            folder.ParentId = parentId;
            folder.Name = name;
            folder.ComputePath();
            folder.CreateInDb(asAdmin);
            if (!pathHelper.UseS3)
            {
                folder.CreateInFs();
            }
            return folder;
        }

        public Folder Update(Folder folder, PathHelper pathHelper)
        {
            folder.ComputePath();
            var updated = UpdateInDb(folder);
            if (!pathHelper.UseS3)
            {
                folder.Move();
            }
            return updated;
        }

        public void Delete(Folder folder, PathHelper pathHelper)
        {
            DeleteFromDb(folder);
            pathHelper.RemoveFolder(folder.PathInfo.Path);
        }
    }
}
