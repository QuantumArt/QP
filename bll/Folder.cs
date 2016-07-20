using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Factories;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Validators;

namespace Quantumart.QP8.BLL
{
	public abstract class Folder : EntityObject
    {

		#region private

        private Folder _ParentFolder;
        private EntityObject _Parent;
        private IEnumerable<EntityObject> _Children;
		private bool _HasChildren;
        private PathInfo _PathInfo;
		private FolderRepository _Repository;

        #endregion

        #region abstract

		protected abstract EntityObject GetParent();

		protected abstract FolderFactory GetFactory();

        #endregion

		#region public properties

		#region overrides

		/// <summary>
		/// Имя папки
		/// </summary>
		[RequiredValidator(MessageTemplateResourceName = "NameNotEntered", MessageTemplateResourceType = typeof(FolderStrings))]
		[FormatValidator(Constants.RegularExpressions.InvalidFolderName, Negated = true, MessageTemplateResourceName = "NameInvalidFormat", MessageTemplateResourceType = typeof(FolderStrings))]
		[LocalizedDisplayName("Name", NameResourceType = typeof(FolderStrings))]
		public override string Name {get;set;}

		/// <summary>
		/// Список дочерних сущностей (папок)
		/// </summary>
		public override IEnumerable<EntityObject> Children
		{
			get
			{
				if (_Children == null && AutoLoadChildren)
				{
					_Children = (LoadAllChildren) ? Repository.GetAllChildrenFromDB(Id) : Repository.GetChildrenFromDB(ParentEntityId, Id);
				}
				return _Children;
			}
		}


		/// <summary>
		/// Информация о путях
		/// </summary>
		public override PathInfo PathInfo
		{
			get
			{
				if (_PathInfo == null)
				{
					_PathInfo = CreatePathInfo(Path);
				}
				return _PathInfo;
			}
		}

		private PathInfo CreatePathInfo(string path)
		{
			return Parent.PathInfo.GetSubPathInfo(path);
		}

		/// <summary>
		/// Признак, определяющий, есть ли у папки дочерние
		/// </summary>
		public override bool HasChildren
		{
		    get
		    {
				return (AutoLoadChildren) ? Children.Any() : _HasChildren;
		    }
			set
			{
				_HasChildren = value;
			}
		}

		/// <summary>
		/// Ссылка на родительскую сущность (не папку)
		/// </summary>
		public override EntityObject Parent
		{
			get
			{
				if (_Parent == null)
				{
					_Parent = GetParent();
				}
				return _Parent;
			}
		}

		public bool IsEmpty
		{
			get
			{
				if (IsNew)
					return true;
				if (!Directory.Exists(PathInfo.Path))
					return true;
				return Directory.EnumerateFileSystemEntries(PathInfo.Path, "*", SearchOption.AllDirectories).Any();
			}

		}

		#endregion

		/// <summary>
		/// путь к директории
		/// </summary>
		public string Path { get; set; }

		/// <summary>
		/// Путь к директории полученный из БД
		/// не меняеться на основе данных формы
		/// </summary>
		public string StoredPath { get; set; }

		/// <summary>
		/// Идентификатор родительской папки
		/// </summary>
		public int? ParentId { get; set; }

		/// <summary>
		/// Признак, определяющий загружать ли дочерние папки по Lazy Load. Иначе предполагается, что они будут получены из базы вместе с родителем
		/// </summary>
		public bool AutoLoadChildren { get; set; }

		/// <summary>
		/// Признак, определяющий загружать ли все дочерние папки (для системных целей) или только те папки, к которым имеет права доступа текущий пользователь
		/// </summary>
		public bool LoadAllChildren { get; set; }

		/// <summary>
		/// Ссылка на репозиторий папок
		/// </summary>
		public FolderRepository Repository
		{
			get
			{
				if (_Repository == null)
				{
					FolderFactory factory = GetFactory();
					_Repository = factory.CreateRepository();
				}
				return _Repository;
			}
		}

		/// <summary>
		/// Ссылка на родительскую папку
		/// </summary>
		public Folder ParentFolder
		{
			get
			{
				if (_ParentFolder == null)
				{
					if (ParentId == null)
						_ParentFolder = null;
					else
						_ParentFolder = Repository.GetById((int)ParentId);
				}
				return _ParentFolder;
			}
		}

		/// <summary>
		/// Имя папки для отображения
		/// </summary>
		public string OutputName
		{
			get
			{
				return (!String.IsNullOrEmpty(Name)) ? Name : LibraryStrings.RootFolder;
			}
		}

		#endregion

		#region public methods

		/// <summary>
		/// Создаем папку в БД
		/// </summary>
		public void CreateInDB(bool asAdmin)
		{
			Folder newFolder = (asAdmin) ? Repository.CreateInDBAsAdmin(this) : Repository.CreateInDB(this);
			this.Id = newFolder.Id;
			this.Path = newFolder.Path;
		}

		/// <summary>
		/// Создаем папку в файловой системе
		/// </summary>
		internal void CreateInFS()
		{
			if (!Directory.Exists(PathInfo.Path))
				Directory.CreateDirectory(PathInfo.Path);
		}

		/// <summary>
		/// Обновляем папку в файловой системе
		/// </summary>
		internal void Move()
		{
			ComputePath();
			PathInfo oldPathInfo = CreatePathInfo(StoredPath);
			if (!Directory.Exists(PathInfo.Path) && Directory.Exists(oldPathInfo.Path))
			{
				Directory.Move(oldPathInfo.Path, PathInfo.Path);
				StoredPath = Path;
			}
		}

		internal void RemoveFromFS()
		{
			if (Directory.Exists(PathInfo.Path))
				Folder.ForceDelete(PathInfo.Path);
		}

		/// <summary>
		/// Создаем недостающие в БД папки на основании файловой системы
		/// </summary>
		internal void CreateChildren(bool stopResursion = false)
		{
			if (Children != null)
			{
				CreateInFS();
				foreach (var child in Children)
				{
					((Folder)child).CreateInFS();
				}

				List<string> childrenNames = Children.Select(n => n.Name.ToLowerInvariant()).ToList();
				IEnumerable<string> namesToCreateInDb = new DirectoryInfo(PathInfo.Path)
					.EnumerateDirectories()
					.Select(n => n.Name)
					.Where(n => !childrenNames.Contains(n.ToLowerInvariant()) && !Folder.IsSpecialName(n))
				;

				foreach (var name in namesToCreateInDb )
				{
					Repository.Create(ParentEntityId, Id, name, true);
				}
			}
		}

		/// <summary>
		/// Проверка, является ли имя папки служебным
		/// </summary>
		/// <param name="name">имя папки</param>
		/// <returns>результат</returns>
		internal static bool IsSpecialName(string name)
		{
			return (name == ArticleVersion.RootFolder || name.StartsWith(Field.Prefix));
		}

		public static void ForceDelete(string path)
		{
			if (Directory.Exists(path))
			{
				var directory = new DirectoryInfo(path) { Attributes = FileAttributes.Normal };

				foreach (var info in directory.EnumerateFileSystemInfos("*", SearchOption.AllDirectories))
				{
					info.Attributes = FileAttributes.Normal;
				}

				directory.Delete(true);
			}

		}

		#endregion

		#region overrided
		protected override void ValidateUnique(RulesException errors)
		{
			if (Directory.Exists(CreatePathInfo(CreateComputedPath(Name)).Path))
				errors.Error("Name", Name, PropertyIsNotUniqueMessage);
		}

		public override int? RecurringId
		{
			get
			{
				return ParentId;
			}
		}
		#endregion

		internal ListResult<FolderFile> GetFiles(ListCommand command, LibraryFileFilter filter)
		{
			var files = new DirectoryInfo(PathInfo.Path).EnumerateFiles(filter.Mask);
			var sort = (String.IsNullOrEmpty(command.SortExpression)) ? "Name ASC" : command.SortExpression;
			Func<FolderFile, bool> typeFilter = (filter != null && filter.FileType.HasValue) ? (Func<FolderFile, bool>)(f => f.FileType == filter.FileType.Value) : f => true;
			var filtered = files
							.Select(n => new FolderFile(n))
							.Where(typeFilter)
							.AsQueryable() // to use dynamic query
							.OrderBy(sort);

			var filteredAndPaged = filtered
							.Skip((command.StartPage - 1) * command.PageSize)
							.Take(command.PageSize)
							.ToList();
			return new ListResult<FolderFile>() { Data = filteredAndPaged, TotalRecords = filtered.Count() };
		}

		internal static PathInfo GetPathInfo(FolderFactory factory, int id)
		{
			Folder folder = factory.CreateRepository().GetById(id);
			return (folder != null) ? folder.PathInfo : null;
		}

		internal Folder TraverseTree(string subFolder)
		{
			Folder currentFolder = this;
			if (!String.IsNullOrEmpty(subFolder))
			{
				string[] names = subFolder.Split('\\');
				int id = Id;
				foreach (var name in names)
				{
					if (!String.IsNullOrEmpty(name))
					{
						currentFolder = Repository.GetChildByName(id, name);
						if (currentFolder == null)
						{
							currentFolder = Repository.Create(ParentEntityId, id, name);
						}
						id = currentFolder.Id;
					}

				}
			}
			return currentFolder;
		}

		internal void ComputePath()
		{
			Path = CreateComputedPath(Name);
		}

		private string CreateComputedPath(string name)
		{
			return String.Format(@"{0}{1}\", (ParentFolder == null) ? String.Empty : ParentFolder.Path, name);
		}
	}
}
