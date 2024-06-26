using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using I = System.IO;
using System.Linq;
using System.Linq.Dynamic;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using NLog;
using Quantumart.QP8.BLL.Factories.FolderFactory;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Utils;

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
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        public PathHelper PathHelper { get; set; }

        protected abstract EntityObject GetParent();

        protected abstract FolderFactory GetFactory();

        [Required(ErrorMessageResourceName = "NameNotEntered", ErrorMessageResourceType = typeof(FolderStrings))]
        [RegularExpression(RegularExpressions.FolderName, ErrorMessageResourceName = "NameInvalidFormat", ErrorMessageResourceType = typeof(FolderStrings))]
        [Display(Name = "Name", ResourceType = typeof(FolderStrings))]
        public override string Name { get; set; }

        [BindNever]
        [ValidateNever]
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

        [BindNever]
        [ValidateNever]
        public override PathInfo PathInfo => _pathInfo ?? (_pathInfo = CreatePathInfo(Path));


        private PathInfo CreatePathInfo(string path) => Parent.PathInfo.GetSubPathInfo(path);

        [BindNever]
        [ValidateNever]
        public override bool HasChildren
        {
            get => AutoLoadChildren ? Children.Any() : _hasChildren;
            set => _hasChildren = value;
        }

        [BindNever]
        [ValidateNever]
        public override EntityObject Parent => _parent ?? (_parent = GetParent());

        public bool IsEmpty(PathHelper pathHelper)
        {
            if (IsNew)
            {
                return true;
            }

            var result = IsNew;
            if (!result)
            {
                result = pathHelper.UseS3 ?
                    pathHelper.ListS3Files(PathInfo.Path, true).Any() :
                    I.Directory.Exists(PathInfo.Path) && I.Directory.EnumerateFileSystemEntries(
                        PathInfo.Path, "*", I.SearchOption.AllDirectories
                    ).Any();
            }
            return result;
        }

        /// <summary>
        /// Путь к директории
        /// </summary>
        public string Path { get; set; }

        public string OsSpecificPath => RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? Path : Path?.Replace(@"\", @"/");

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

        [BindNever]
        [ValidateNever]
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

        [BindNever]
        [ValidateNever]
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
            if (!I.Directory.Exists(PathInfo.Path))
            {
                I.Directory.CreateDirectory(PathInfo.Path);
            }
        }

        internal void Move()
        {
            ComputePath();
            var oldPathInfo = CreatePathInfo(StoredPath);
            if (!I.Directory.Exists(PathInfo.Path) && I.Directory.Exists(oldPathInfo.Path))
            {
                I.Directory.Move(oldPathInfo.Path, PathInfo.Path);
                StoredPath = Path;
            }
        }

        /// <summary>
        /// Создаем недостающие в БД папки на основании файловой системы
        /// </summary>
        internal void CreateChildren(PathHelper pathHelper)
        {
            if (Children != null)
            {
                if (!pathHelper.UseS3)
                {
                    CreateInFs();
                    foreach (var child in Children)
                    {
                        ((Folder)child).CreateInFs();
                    }
                }

                var childrenNames = Children.Select(n => n.Name.ToLowerInvariant()).ToList();
                IEnumerable<string> namesToCreateInDb;
                if (pathHelper.UseS3)
                {
                    try
                    {
                        namesToCreateInDb = pathHelper.ListS3Files(PathInfo.Path, onlyDirs: true)
                            .Select(n => n.Name.Left(n.Name.Length - 1));
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                        namesToCreateInDb = Array.Empty<string>();
                    }
                }
                else
                {
                    namesToCreateInDb = new I.DirectoryInfo(PathInfo.Path)
                        .EnumerateDirectories()
                        .Select(n => n.Name);
                }

                namesToCreateInDb = namesToCreateInDb
                    .Where(n => !childrenNames.Contains(n.ToLowerInvariant()) && !IsSpecialName(n)).ToArray();

                foreach (var name in namesToCreateInDb)
                {
                    Repository.Create(ParentEntityId, Id, name, pathHelper, true);
                }
            }
        }

        internal static bool IsSpecialName(string name) => name == ArticleVersion.RootFolder || name.StartsWith(Field.Prefix);


        protected override RulesException ValidateUnique(RulesException errors)
        {
            if (I.Directory.Exists(CreatePathInfo(CreateComputedPath(Name)).Path))
            {
                errors.Error("Name", Name, PropertyIsNotUniqueMessage);
            }

            return errors;
        }

        public override int? RecurringId => ParentId;

        internal ListResult<FolderFile> GetFiles(ListCommand command, LibraryFileFilter filter)
        {
            IEnumerable<FolderFile> list;
            var sort = string.IsNullOrEmpty(command.SortExpression) ? "Name ASC" : command.SortExpression;
            if (PathHelper.UseS3)
            {
                list = PathHelper.ListS3Files(PathInfo.Path, pattern: filter.Mask.Replace("*", ""));
            }
            else
            {
                list = new I.DirectoryInfo(PathInfo.Path)
                    .EnumerateFiles(filter.Mask).Select(n => new FolderFile(n));
            }

            var typeFilter = filter.FileType.HasValue ? (Func<FolderFile, bool>)(f => f.FileType == filter.FileType.Value) : f => true;
            var filtered = list.Where(typeFilter)
                .AsQueryable()
                .OrderBy(sort);

            var filteredAndPaged = filtered
                .Skip((command.StartPage - 1) * command.PageSize)
                .Take(command.PageSize)
                .ToList();

            return new ListResult<FolderFile> { Data = filteredAndPaged, TotalRecords = filtered.Count() };
        }

        internal static PathInfo GetPathInfo(FolderFactory factory, int id) => factory.CreateRepository().GetById(id)?.PathInfo;

        internal Folder TraverseTree(string subFolder, PathHelper pathHelper)
        {
            var currentFolder = this;
            if (!string.IsNullOrEmpty(subFolder))
            {
                var names = subFolder.Replace('\\', I.Path.DirectorySeparatorChar).Split(I.Path.DirectorySeparatorChar);
                var id = Id;
                foreach (var name in names)
                {
                    if (!string.IsNullOrEmpty(name))
                    {
                        currentFolder = Repository.GetChildByName(id, name) ?? Repository.Create(ParentEntityId, id, name, pathHelper);
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

        private string CreateComputedPath(string name) => $@"{(ParentFolder == null ? string.Empty : ParentFolder.Path)}{name}" + @"\";
    }
}
