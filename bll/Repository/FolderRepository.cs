using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.BLL.Factories;

namespace Quantumart.QP8.BLL.Repository
{
	public abstract class FolderRepository
	{
		#region private

		private Folder GetOrCreateRoot(int parentEntityId)
		{
			Folder folder = GetRoot(parentEntityId);
			if (folder == null)
				folder = (Folder)CreateRoot(parentEntityId, true);
			else
				folder.CreateInFS();
			return folder;
		}

		private Folder CreateRoot(int parentEntityId, bool asAdmin = false)
		{
			return Create(parentEntityId, null, String.Empty, asAdmin);
		}

		private Folder Get(int parentEntityId, int? parentId)
		{
			Folder folder;
			if (parentId == null)
			{
				folder = GetOrCreateRoot(parentEntityId);
			}
			else
			{
				folder = GetById((int)parentId);
				if (folder == null)
					throw new ArgumentException(String.Format(FolderNotFoundMessage(parentId.Value)));
			}
			folder.CreateInFS();
			folder.AutoLoadChildren = true;
			return folder;
		}

		protected int Synchronize(int parentEntityId, int? parentId)
		{
			Folder folder = Get(parentEntityId, parentId);
			folder.LoadAllChildren = true;
			folder.CreateChildren();
			return folder.Id;
		}

		public Folder GetSelfAndChildrenWithSync(int parentEntityId, int? parentId)
		{
			Synchronize(parentEntityId, parentId);
			Folder folder = Get(parentEntityId, parentId);
			return folder;
		}

		#endregion
		
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
		public abstract Folder CreateInDB(Folder folder);

		/// <summary>
		/// Создает новую папку в БД от имени администратора
		/// </summary>
		/// <param name="folder">папка</param>
		/// <returns>сохраненная папка (имеет ID)</returns>
		public abstract Folder CreateInDBAsAdmin(Folder folder);

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
		public abstract IEnumerable<Folder> GetAllChildrenFromDB(int parentId);


		/// <summary>
		/// Загружает дочерние папки из базы данных
		/// </summary>
		/// <param name="parentEntityId">ID родительской сущности (сайта или контента)</param>
		/// <param name="parentId">ID папки</param>
		/// <returns>список дочерних папок</returns>
		public abstract IEnumerable<Folder> GetChildrenFromDB(int parentEntityId, int parentId);


		/// <summary>
		/// Получает дочерние папки с учетом прав доступа текущего пользователя. При этом выполняется синхронизация.
		/// </summary>
		/// <param name="parentEntityId">ID родительской сущности (сайта или контента)</param>
		/// <param name="parentId">ID папки</param>
		/// <returns>список дочерних папок</returns>
		public abstract IEnumerable<Folder> GetChildrenWithSync(int parentEntityId, int? parentId);

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
		/// <returns>папка</returns>
		public Folder GetBySubFolder(int parentEntityId, string subFolder)
		{
			return GetOrCreateRoot(parentEntityId).TraverseTree(subFolder);
		}

		/// <summary>
		/// Создание новой папки (в БД и в файловой системе)
		/// </summary>
		/// <param name="parentEntityId">ID родительской сущности(сайта или контента)</param>
		/// <param name="parentId">ID родительской папки</param>
		/// <param name="name">имя папки</param>
		/// <param name="asAdmin">признак, создавать ли папку от имени админа (необходимо для фонового создания папок)</param>
		/// <returns></returns>
		public Folder Create(int parentEntityId, int? parentId, string name, bool asAdmin = false)
		{
			Folder folder = Factory.CreateFolder();
			folder.ParentEntityId = parentEntityId;
			folder.ParentId = parentId;
			folder.Name = name;
			folder.ComputePath();
			folder.CreateInDB(asAdmin);
			folder.CreateInFS();
			return folder;
		}

		public Folder Update(Folder folder)
		{			
			folder.ComputePath();
			Folder uodatedFolder = UpdateInDb(folder);
			folder.Move();
			return uodatedFolder;
		}

		public void Delete(Folder folder)
		{						
			DeleteFromDb(folder);
			folder.RemoveFromFS();			
		}		
	}
}
