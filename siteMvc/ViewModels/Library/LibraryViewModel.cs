using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using Quantumart.QP8.WebMvc.ViewModels.Abstract;

namespace Quantumart.QP8.WebMvc.ViewModels
{
    public class LibraryViewModel : ViewModel
    {
        private static readonly List<QPSelectListItem> FileTypeList = GetFileTypeList();

        private static List<QPSelectListItem> GetFileTypeList()
        {
            var list = new List<QPSelectListItem>
            {
                GetFileTypeListItem(FolderFileType.CSS),
                GetFileTypeListItem(FolderFileType.Flash),
                GetFileTypeListItem(FolderFileType.Image),
                GetFileTypeListItem(FolderFileType.Javascript),
                GetFileTypeListItem(FolderFileType.Media),
                GetFileTypeListItem(FolderFileType.Office),
                GetFileTypeListItem(FolderFileType.PDF)
            };

            return list;
        }

        private static QPSelectListItem GetFileTypeListItem(FolderFileType type)
        {
            return new QPSelectListItem
            {
                Text = FolderFile.GetTypeName(type),
                Value = ((int)type).ToString()
            };
        }

        public LibraryViewModel()
        {
            Mode = LibraryMode.Site;
        }

        public static LibraryViewModel Create(LibraryResult result, string tabId, int parentId, int? filterFileTypeId, bool allowUpload, LibraryMode mode)
        {
            var model = Create<LibraryViewModel>(tabId, parentId);
            model.Mode = mode;
            model.RootFolder = result.Folder;
            model.FilterFileTypeId = filterFileTypeId;
            model.AllowUpload = allowUpload;
            return model;
        }

        /// <summary>
        /// Библиотека сайта или контента
        /// </summary>
        public LibraryMode Mode { get; set; }

        /// <summary>
        /// корневая папка библиотеки
        /// </summary>
        public Folder RootFolder { get; set; }

        /// <summary>
        /// Предопределенный фильтр по типу файлов
        /// </summary>
        public int? FilterFileTypeId { get; set; }

        public bool AllowUpload { get; set; }

        public string FolderEntityTypeCode => Mode == LibraryMode.Site ? Constants.EntityTypeCode.SiteFolder : Constants.EntityTypeCode.ContentFolder;

        public string FileEntityTypeCode => Mode == LibraryMode.Site ? Constants.EntityTypeCode.SiteFile : Constants.EntityTypeCode.ContentFile;

        public string FolderContextMenuCode => FolderEntityTypeCode;

        public string FileContextMenuCode => FileEntityTypeCode;

        public string SplitterId => UniqueId("Splitter");

        public string ContentElementId => UniqueId("Content");

        public string TreeElementId => UniqueId("Tree");

        public string TreeContainerElementId => UniqueId("TreeContainer");

        public string GridContainerElementId => UniqueId("GridContainer");

        public string ListContainerElementId => UniqueId("ListContainer");

        public string ThumbContainerElementId => UniqueId("ThumbContainer");

        public string GridElementId => UniqueId("Grid");

        public string ControllerName => Mode == LibraryMode.Site ? "Site" : "Content";

        public IEnumerable<QPSelectListItem> FileTypes
        {
            get
            {
                return FileTypeList.OrderBy(i => i.Value).ToArray();
            }
        }

        public UploaderType UploaderType => UploaderTypeHelper.UploaderType;

        public override string EntityTypeCode => Mode == LibraryMode.Site ? Constants.EntityTypeCode.Site : Constants.EntityTypeCode.Content;

        public override string ActionCode => Mode == LibraryMode.Site ? Constants.ActionCode.SiteLibrary : Constants.ActionCode.ContentLibrary;

        public override MainComponentType MainComponentType => MainComponentType.Library;

        public override string MainComponentId => UniqueId("Library");

        public override ExpandoObject MainComponentOptions
        {
            get
            {
                dynamic result = base.MainComponentOptions;
                result.folderContextMenuCode = FolderContextMenuCode;
                result.fileContextMenuCode = FileContextMenuCode;
                result.folderId = RootFolder.Id;
                result.splitterId = SplitterId;
                result.fileGridId = GridElementId;
                result.folderTreeId = TreeElementId;
                result.allowMultipleSelection = !IsWindow;
                result.uploaderType = (int)UploaderType;
                result.allowUpload = AllowUpload;
                if (FilterFileTypeId.HasValue)
                {
                    result.filterFileTypeId = (int)FilterFileTypeId;
                }

                return result;
            }
        }
    }
}
