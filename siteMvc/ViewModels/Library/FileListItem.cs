using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Quantumart.QP8.Configuration;
using System.IO;
using Quantumart.QP8.BLL;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.WebMvc.ViewModels
{
	/// <summary>
	/// View Model элемента для списков файлов
	/// </summary>
	public class FileListItem
	{
		#region private

		private static Dictionary<FolderFileType, string> fileTypeSmallIconDictionary = new Dictionary<FolderFileType, string>
		{
			{FolderFileType.CSS, "css_16.png"},
			{FolderFileType.Flash, "flash_16.png"},
			{FolderFileType.Image, "image_16.png"},
			{FolderFileType.Javascript, "javascript_16.png"},
			{FolderFileType.Media, "media_16.png"},
			{FolderFileType.Office, "office_16.png"},
			{FolderFileType.PDF, "pdf_16.png"},
			{FolderFileType.Unknown, "unknown_16.png"}
		};

		private static Dictionary<FolderFileType, string> fileTypeBigIconDictionary = new Dictionary<FolderFileType, string>
		{
			{FolderFileType.CSS, "css_64.png"},
			{FolderFileType.Flash, "flash_64.png"},
			{FolderFileType.Image, "image_64.png"},
			{FolderFileType.Javascript, "javascript_64.png"},
			{FolderFileType.Media, "media_64.png"},
			{FolderFileType.Office, "office_64.png"},
			{FolderFileType.PDF, "pdf_64.png"},
			{FolderFileType.Unknown, "unknown_64.png"}
		};

		#endregion

		#region creation

		public static FileListItem Create(FolderFile file, int fileShortNameLength)
		{
			FileListItem item = new FileListItem();
			item.FullName = file.Name;
			item.Name = String.Concat(Typographer.CutShort(Path.GetFileNameWithoutExtension(file.Name), fileShortNameLength), Path.GetExtension(file.Name));
			item.Size = file.Size;
			item.Modified = file.Modified.ToLongTimeString();
			item.FileType = file.FileType;
			return item;
		}

		#endregion
		/// <summary>
		/// Укороченное имя файла с расширением
		/// </summary>
		public string Name { get; set; }
		/// <summary>
		/// Полное имя файла с расширением
		/// </summary>
		public string FullName { get; set; }		
		/// <summary>
		/// Размер файла
		/// </summary>
		public string Size { get; set; }
		/// <summary>
		/// Дата создания файла
		/// </summary>
		public string Created { get; set; }
		/// <summary>
		/// Дата модификации файла
		/// </summary>
		public string Modified { get; set; }
		/// <summary>
		/// Тип файла
		/// </summary>		
		public FolderFileType FileType { get; set; }
		/// <summary>
		/// Ссылка на картинку с маленькой иконкой
		/// </summary>
		public string SmallIconLink 
		{ 
			get 
			{
				return fileTypeSmallIconDictionary[this.FileType];
			} 
		}
		/// <summary>
		/// Ссылка на картинку для preview
		/// </summary>
		public string BigIconLink
		{
			get
			{								
				return fileTypeBigIconDictionary[this.FileType];
			}
		} 
	}
}