using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using B = Quantumart.QP8.BLL;
using Quantumart.QP8.Resources;
using C = Quantumart.QP8.Constants;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using Quantumart.QP8.BLL;
using System.Dynamic;

namespace Quantumart.QP8.WebMvc.ViewModels
{
	
	public enum LibraryMode
	{
		Site,
		Content
	}	
	
	public class LibraryViewModel : ViewModel
	{

		#region private

		private static List<QPSelectListItem> FileTypeList = GetFileTypeList();
		
		private static List<QPSelectListItem> GetFileTypeList()
		{
				List<QPSelectListItem> list = new List<QPSelectListItem>();
				list.Add(GetFileTypeListItem(FolderFileType.CSS));
				list.Add(GetFileTypeListItem(FolderFileType.Flash));
				list.Add(GetFileTypeListItem(FolderFileType.Image));
				list.Add(GetFileTypeListItem(FolderFileType.Javascript));
				list.Add(GetFileTypeListItem(FolderFileType.Media));
				list.Add(GetFileTypeListItem(FolderFileType.Office));
				list.Add(GetFileTypeListItem(FolderFileType.PDF));
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

		#endregion

		#region creation

		public LibraryViewModel()
		{
			Mode = LibraryMode.Site;
		}

		public static LibraryViewModel Create(LibraryResult result, string tabId, int parentId, int? filterFileTypeId, bool allowUpload, LibraryMode mode)
		{
			LibraryViewModel model = ViewModel.Create<LibraryViewModel>(tabId, parentId);
			model.Mode = mode;
			model.RootFolder = result.Folder;
			model.FilterFileTypeId = filterFileTypeId;
			model.AllowUpload = allowUpload;
			return model;
		}

		#endregion

	
		/// <summary>
		/// Библиотека сайта или контента
		/// </summary>
		public LibraryMode Mode { get; set; }

		/// <summary>
		/// корневая папка библиотеки
		/// </summary>
		public B.Folder RootFolder { get; set; }

		/// <summary>
		/// Предопределенный фильтр по типу файлов
		/// </summary>
		public int? FilterFileTypeId { get; set; }

		public bool AllowUpload { get; set; }

		public string FolderEntityTypeCode
		{
			get
			{
				return (Mode == LibraryMode.Site) ? C.EntityTypeCode.SiteFolder : C.EntityTypeCode.ContentFolder;
			}
		}

		public string FileEntityTypeCode
		{
			get
			{
				return (Mode == LibraryMode.Site) ? C.EntityTypeCode.SiteFile : C.EntityTypeCode.ContentFile;
			}
		}

		public string FolderContextMenuCode
		{
			get
			{
				return FolderEntityTypeCode;
			}
		}

		public string FileContextMenuCode
		{
			get
			{
				return FileEntityTypeCode;
			}
		}
		
		public string SplitterId
		{
			get
			{
				return UniqueId("Splitter");
			}
		}

		public string ContentElementId
		{
			get
			{
				return UniqueId("Content");
			}
		}

		public string TreeElementId
		{
			get
			{
				return UniqueId("Tree");
			}
		}

		public string TreeContainerElementId
		{
			get
			{
				return UniqueId("TreeContainer");
			}
		}

		public string GridContainerElementId
		{
			get
			{
				return UniqueId("GridContainer");
			}
		}

		public string ListContainerElementId
		{
			get
			{
				return UniqueId("ListContainer");
			}
		}

		public string ThumbContainerElementId
		{
			get
			{
				return UniqueId("ThumbContainer");
			}
		}

		public string GridElementId
		{
			get
			{
				return UniqueId("Grid");
			}
		}

		public string ControllerName
		{
			get
			{
				return (Mode == LibraryMode.Site) ? "Site" : "Content";
			}
		}

		public IEnumerable<QPSelectListItem> FileTypes
		{
			get
			{
				return FileTypeList.OrderBy(i => i.Value).ToArray();
			}
		}

		public UploaderType UploaderType
		{
			get
			{
				return UploaderTypeHelper.UploaderType;
			}
		}

		#region overrides

		public override string EntityTypeCode
		{
			get
			{
				return (Mode == LibraryMode.Site) ? C.EntityTypeCode.Site : C.EntityTypeCode.Content;
			}
		}

		public override string ActionCode
		{
			get
			{
				return (Mode == LibraryMode.Site) ? C.ActionCode.SiteLibrary : C.ActionCode.ContentLibrary;
			}
		}

		public override MainComponentType MainComponentType
		{
			get { return Constants.MainComponentType.Library; }
		}

		public override string MainComponentId
		{
			get { return UniqueId("Library"); }
		}

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
					result.filterFileTypeId = (int)FilterFileTypeId;
				return result;
			}
		}


		#endregion

	}
}
