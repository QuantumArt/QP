using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Linq.Dynamic;
using Quantumart.QP8.BLL.Factories.FolderFactory;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL
{
    public abstract class Folder : EntityObject
    {
        private Folder _parentFolder;
        private EntityObject _parent;
        private IEnumerable<EntityObject> _children;
        private bool _hasChildren;
        private PathInfo _pathInfo;
        private FolderRepository _repository;

        protected abstract EntityObject GetParent();

        protected abstract FolderFactory GetFactory();

        [Required(ErrorMessageResourceName = "NameNotEntered", ErrorMessageResourceType = typeof(FolderStrings))]
        [RegularExpression(RegularExpressions.FolderName, ErrorMessageResourceName = "NameInvalidFormat", ErrorMessageResourceType = typeof(FolderStrings))]
        [Display(Name = "Name", ResourceType = typeof(FolderStrings))]
        public override string Name { get; set; }

        public override IEnumerable<EntityObject> Children
        {
            get
            {
                if (_children == null && AutoLoadChildren)
                {
                    _children = LoadAllChildren ? Repository.GetAllChildrenFromDb(Id) : Repository.GetChildrenFromDb(ParentEntityId, Id);
                }

                return _children;
            }
        }

        public override PathInfo PathInfo => _pathInfo ?? (_pathInfo = CreatePathInfo(Path));

        private PathInfo CreatePathInfo(string path) => Parent.PathInfo.GetSubPathInfo(path);

        public override bool HasChildren
        {
            get => AutoLoadChildren ? Children.Any() : _hasChildren;
            set => _hasChildren = value;
        }

        public override EntityObject Parent => _parent ?? (_parent = GetParent());

        public bool IsEmpty
        {
            get
            {
                if (IsNew)
                {
                    return true;
                }

                return IsNew || !Directory.Exists(PathInfo.Path) || Directory.EnumerateFileSystemEntries(PathInfo.Path, "*", SearchOption.AllDirectories).Any();
            }
        }

        /// <summary>
        /// Путь к директории
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Путь к директории полученный из БД не меняеться на основе данных формы
        /// </summary>
        public string StoredPath { get; set; }

        public int? ParentId { get; set; }

        /// <summary>
        /// Признак, определяющий загружать ли дочерние папки по Lazy Load. Иначе предполагается, что они будут получены из базы вместе с родителем
        /// </summary>
        public bool AutoLoadChildren { get; set; }

        /// <summary>
        /// Признак, определяющий загружать ли все дочерние папки (для системных целей) или только те папки, к которым имеет права доступа текущий пользователь
        /// </summary>
        public bool LoadAllChildren { get; set; }

        public FolderRepository Repository
        {
            get
            {
                if (_repository == null)
                {
                    var factory = GetFactory();
                    _repository = factory.CreateRepository();
                }

                return _repository;
            }
        }

        public Folder ParentFolder => _parentFolder ?? (_parentFolder = ParentId == null ? null : Repository.GetById((int)ParentId));

        public string OutputName => !string.IsNullOrEmpty(Name) ? Name : LibraryStrings.RootFolder;

        public void CreateInDb(bool asAdmin)
        {
            var newFolder = asAdmin ? Repository.CreateInDbAsAdmin(this) : Repository.CreateInDb(this);
            Id = newFolder.Id;
            Path = newFolder.Path;
        }

        internal void CreateInFs()
        {
            if (!Directory.Exists(PathInfo.Path))
            {
                Directory.CreateDirectory(PathInfo.Path);
            }
        }

        internal void Move()
        {
            ComputePath();
            var oldPathInfo = CreatePathInfo(StoredPath);
            if (!Directory.Exists(PathInfo.Path) && Directory.Exists(oldPathInfo.Path))
            {
                Directory.Move(oldPathInfo.Path, PathInfo.Path);
                StoredPath = Path;
            }
        }

        internal void RemoveFromFs()
        {
            if (Directory.Exists(PathInfo.Path))
            {
                ForceDelete(PathInfo.Path);
            }
        }

        /// <summary>
        /// Создаем недостающие в БД папки на основании файловой системы
        /// </summary>
        internal void CreateChildren(bool stopResursion = false)
        {
            if (Children != null)
            {
                CreateInFs();
                foreach (var child in Children)
                {
                    ((Folder)child).CreateInFs();
                }

                var childrenNames = Children.Select(n => n.Name.ToLowerInvariant()).ToList();
                var namesToCreateInDb = new DirectoryInfo(PathInfo.Path)
                    .EnumerateDirectories()
                    .Select(n => n.Name)
                    .Where(n => !childrenNames.Contains(n.ToLowerInvariant()) && !IsSpecialName(n));

                foreach (var name in namesToCreateInDb)
                {
                    Repository.Create(ParentEntityId, Id, name, true);
                }
            }
        }

        internal static bool IsSpecialName(string name) => name == ArticleVersion.RootFolder || name.StartsWith(Field.Prefix);

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

        protected override RulesException ValidateUnique(RulesException errors)
        {
            if (Directory.Exists(CreatePathInfo(CreateComputedPath(Name)).Path))
            {
                errors.Error("Name", Name, PropertyIsNotUniqueMessage);
            }

            return errors;
        }

        public override int? RecurringId => ParentId;

        internal ListResult<FolderFile> GetFiles(ListCommand command, LibraryFileFilter filter)
        {
            var files = new DirectoryInfo(PathInfo.Path).EnumerateFiles(filter.Mask);
            var sort = string.IsNullOrEmpty(command.SortExpression) ? "Name ASC" : command.SortExpression;
            var typeFilter = filter.FileType.HasValue ? (Func<FolderFile, bool>)(f => f.FileType == filter.FileType.Value) : f => true;
            var filtered = files
                .Select(n => new FolderFile(n))
                .Where(typeFilter)
                .AsQueryable()
                .OrderBy(sort);

            var filteredAndPaged = filtered
                .Skip((command.StartPage - 1) * command.PageSize)
                .Take(command.PageSize)
                .ToList();

            return new ListResult<FolderFile> { Data = filteredAndPaged, TotalRecords = filtered.Count() };
        }

        internal static PathInfo GetPathInfo(FolderFactory factory, int id) => factory.CreateRepository().GetById(id)?.PathInfo;

        internal Folder TraverseTree(string subFolder)
        {
            var currentFolder = this;
            if (!string.IsNullOrEmpty(subFolder))
            {
                var names = subFolder.Split('\\');
                var id = Id;
                foreach (var name in names)
                {
                    if (!string.IsNullOrEmpty(name))
                    {
                        currentFolder = Repository.GetChildByName(id, name) ?? Repository.Create(ParentEntityId, id, name);
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

        private string CreateComputedPath(string name) => $@"{(ParentFolder == null ? string.Empty : ParentFolder.Path)}{name}\";
    }
}
